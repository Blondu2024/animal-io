using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;

// One-shot assembly: builds URP materials from Meshy PBR textures, Animator controllers from Mixamo
// FBX sub-asset clips, and character/prop prefabs. The plain MCP API can't bind FBX sub-asset clips
// or batch this. Runs via [DidReloadScripts] (EditorApplication.update/delayCall are frozen on an
// unfocused Cloud PC editor). Trigger a run: refresh_unity scope=all compile=request. Bump
// BUILD_VERSION to force a re-run. Editor-only; safe to delete when assembly is done.
public static class AnimalAssembly
{
    const int BUILD_VERSION = 7;

    [DidReloadScripts]
    static void OnReload()
    {
        if (EditorPrefs.GetInt("AnimalAssembly_v", 0) >= BUILD_VERSION) return;
        try { RunBuild(); EditorPrefs.SetInt("AnimalAssembly_v", BUILD_VERSION); }
        catch (Exception e) { Debug.LogError("[AnimalAssembly] FAILED: " + e); }
    }

    const string TX = "Assets/Meshy/Textures/";
    const string MAT = "Assets/Meshy/Materials/";

    static void RunBuild()
    {
        Debug.Log("[AnimalAssembly] RunBuild start v" + BUILD_VERSION);
        EnsureFolder(MAT);
        EnsureFolder("Assets/Resources/");

        // ---- materials (base + normal + optional emission) ----
        BuildMaterial(MAT + "Boss_Orangutan.mat", "Boss/boss_base_color.png", "Boss/boss_normal.png", "Boss/boss_emission.png");
        BuildMaterial(MAT + "Monkey.mat",  "monkey/monkey_base_color.png",  "monkey/monkey_normal.png",  "monkey/monkey_emission.png");
        BuildMaterial(MAT + "Barrel.mat",  "barrel/barrel_base_color.png",  "barrel/barrel_normal.png",  null);
        BuildMaterial(MAT + "Spikes.mat",  "spikes/spikes_base_color.png",  "spikes/spikes_normal.png",  null);
        BuildMaterial(MAT + "Chest.mat",   "chest/chest_base_color.png",    "chest/chest_normal.png",    null);
        BuildMaterial(MAT + "Brazier.mat", "brazier/brazier_base_color.png","brazier/brazier_normal.png","brazier/brazier_emission.png");
        BuildMaterial(MAT + "Bridge.mat",  "bridge/bridge_base_color.png",  "bridge/bridge_normal.png",  null);

        // ---- boss: controller + prefab ----
        BuildController("Assets/Meshy/Boss/Boss.controller",
            "Assets/Meshy/Boss/boss_walk.fbx", "Assets/Meshy/Boss/boss_attack.fbx", "Assets/Meshy/Boss/boss_die.fbx");
        BuildCharacterPrefab("Assets/Resources/Boss_Orangutan.prefab",
            "Assets/Meshy/Boss/boss_walk.fbx", MAT + "Boss_Orangutan.mat", "Assets/Meshy/Boss/Boss.controller", 1.0f);

        // ---- monkey: controller (run + attack, no die) + prefab ----
        BuildController("Assets/Meshy/Monkey/Monkey.controller",
            "Assets/Meshy/Monkey/monkey_run.fbx", "Assets/Meshy/Monkey/monkey_attack.fbx", null);
        BuildCharacterPrefab("Assets/Resources/Monkey.prefab",
            "Assets/Meshy/Monkey/monkey_run.fbx", MAT + "Monkey.mat", "Assets/Meshy/Monkey/Monkey.controller", 1.0f);

        // ---- props: static textured prefabs ----
        BuildPropPrefab("Assets/Meshy/Props/Barrel.prefab",  "Assets/Meshy/Props/prop_barrel.fbx",       MAT + "Barrel.mat");
        BuildPropPrefab("Assets/Meshy/Props/Spikes.prefab",  "Assets/Meshy/Props/prop_spikes.fbx",       MAT + "Spikes.mat");
        BuildPropPrefab("Assets/Meshy/Props/Chest.prefab",   "Assets/Meshy/Props/prop_chest.fbx",        MAT + "Chest.mat");
        BuildPropPrefab("Assets/Meshy/Props/Brazier.prefab", "Assets/Meshy/Props/prop_brazier.fbx",      MAT + "Brazier.mat");
        BuildPropPrefab("Assets/Meshy/Props/Bridge.prefab",  "Assets/Meshy/Props/prop_bridge_plank.fbx", MAT + "Bridge.mat");

        AssetDatabase.SaveAssets();

        PlaceProps();
        Debug.Log("[AnimalAssembly] RunBuild done");
    }

    // ---------- scene: attach textured prop visuals onto the grey hazard cubes ----------
    static void PlaceProps()
    {
        int n = 0;
        foreach (var s in UnityEngine.Object.FindObjectsByType<SpikeTrap>(FindObjectsSortMode.None))
            if (AttachProp(s.gameObject, "Assets/Meshy/Props/Spikes.prefab", 1.4f, true, true)) n++;
        foreach (var f in UnityEngine.Object.FindObjectsByType<FireTrap>(FindObjectsSortMode.None))
            if (AttachProp(f.gameObject, "Assets/Meshy/Props/Brazier.prefab", 1.1f, false, true)) n++; // keep flame cube
        foreach (var r in UnityEngine.Object.FindObjectsByType<RollingHazard>(FindObjectsSortMode.None))
            if (AttachProp(r.gameObject, "Assets/Meshy/Props/Barrel.prefab", 1.6f, true, true)) n++;
        foreach (var it in UnityEngine.Object.FindObjectsByType<Interactable>(FindObjectsSortMode.None))
            if (AttachProp(it.gameObject, "Assets/Meshy/Props/Chest.prefab", 1.0f, true, true)) n++;
        foreach (var cb in UnityEngine.Object.FindObjectsByType<CollapseBridge>(FindObjectsSortMode.None))
            if (cb.segments != null)
                foreach (var seg in cb.segments)
                    if (seg != null && AttachProp(seg.gameObject, "Assets/Meshy/Props/Bridge.prefab", 0f, true, true, true)) n++;

        if (n > 0) { EditorSceneManager.MarkAllScenesDirty(); EditorSceneManager.SaveOpenScenes(); }
        Debug.Log("[AnimalAssembly] PlaceProps attached " + n + " props");
    }

    static bool AttachProp(GameObject host, string propPath, float worldSize, bool hideOriginal, bool parentToHost, bool fitToHost = false)
    {
        if (host.transform.Find("_propvis") != null) return false;            // idempotent
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(propPath);
        if (prefab == null) { Debug.LogWarning("[AnimalAssembly] no prop " + propPath); return false; }
        var prop = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        prop.name = "_propvis";
        prop.transform.position = host.transform.position;
        prop.transform.rotation = host.transform.rotation;

        // auto-size from renderer bounds (props import tiny*100, so size explicitly)
        var rends = prop.GetComponentsInChildren<Renderer>();
        if (rends.Length > 0)
        {
            Bounds b = rends[0].bounds; for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            float cur = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
            float target = worldSize;
            if (fitToHost) { var hr = host.GetComponent<Renderer>(); if (hr != null) target = Mathf.Max(hr.bounds.size.x, hr.bounds.size.z); }
            if (cur > 0.0001f && target > 0.0001f) prop.transform.localScale *= target / cur;
        }
        if (parentToHost) prop.transform.SetParent(host.transform, true);     // follow movement, keep world transform
        if (hideOriginal) { var mr = host.GetComponent<MeshRenderer>(); if (mr != null) mr.enabled = false; }
        return true;
    }

    // ---------- materials ----------
    static void BuildMaterial(string matPath, string baseRel, string normalRel, string emisRel)
    {
        try
        {
            var sh = Shader.Find("Universal Render Pipeline/Lit");
            var mat = new Material(sh);
            var baseTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TX + baseRel);
            if (baseTex != null) mat.SetTexture("_BaseMap", baseTex);

            if (!string.IsNullOrEmpty(normalRel))
            {
                MarkNormal(TX + normalRel);
                var n = AssetDatabase.LoadAssetAtPath<Texture2D>(TX + normalRel);
                if (n != null) { mat.SetTexture("_BumpMap", n); mat.EnableKeyword("_NORMALMAP"); mat.SetFloat("_BumpScale", 1f); }
            }
            if (!string.IsNullOrEmpty(emisRel))
            {
                var e = AssetDatabase.LoadAssetAtPath<Texture2D>(TX + emisRel);
                if (e != null)
                {
                    mat.SetTexture("_EmissionMap", e);
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", Color.white * 1.5f);
                    mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                }
            }
            mat.SetFloat("_Smoothness", 0.25f);
            mat.SetFloat("_Metallic", 0f);
            AssetDatabase.DeleteAsset(matPath);
            AssetDatabase.CreateAsset(mat, matPath);
            Debug.Log($"[AnimalAssembly] mat {matPath}: base={baseTex!=null}");
        }
        catch (Exception e) { Debug.LogError("[AnimalAssembly] mat " + matPath + " FAILED: " + e.Message); }
    }

    static void MarkNormal(string path)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null && ti.textureType != TextureImporterType.NormalMap)
        { ti.textureType = TextureImporterType.NormalMap; ti.SaveAndReimport(); }
    }

    // ---------- animation helpers ----------
    static AnimationClip LoadClip(string fbx)
    {
        return AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<AnimationClip>()
            .FirstOrDefault(c => c != null && !c.name.StartsWith("__preview"));
    }
    static Avatar LoadAvatar(string fbx) => AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<Avatar>().FirstOrDefault();

    static void SetClipLoop(string fbx, bool loop)
    {
        try
        {
            var imp = AssetImporter.GetAtPath(fbx) as ModelImporter;
            if (imp == null) return;
            var clips = imp.defaultClipAnimations;
            if (clips == null || clips.Length == 0) return;
            for (int i = 0; i < clips.Length; i++) clips[i].loopTime = loop;
            imp.clipAnimations = clips;
            imp.SaveAndReimport();
        }
        catch (Exception e) { Debug.LogWarning("[AnimalAssembly] loop " + fbx + ": " + e.Message); }
    }

    static AnimatorController BuildController(string path, string locoFbx, string attackFbx, string dieFbx)
    {
        AssetDatabase.DeleteAsset(path);
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);
        ctrl.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Die", AnimatorControllerParameterType.Trigger);
        var sm = ctrl.layers[0].stateMachine;

        SetClipLoop(locoFbx, true);
        var loco = sm.AddState("Move");
        loco.motion = LoadClip(locoFbx);
        sm.defaultState = loco;

        if (!string.IsNullOrEmpty(attackFbx))
        {
            var atk = sm.AddState("Attack");
            atk.motion = LoadClip(attackFbx);
            var toAtk = loco.AddTransition(atk);
            toAtk.AddCondition(AnimatorConditionMode.If, 0, "Attack");
            toAtk.hasExitTime = false; toAtk.duration = 0.08f;
            var back = atk.AddTransition(loco);
            back.hasExitTime = true; back.exitTime = 0.85f; back.duration = 0.12f;
        }
        if (!string.IsNullOrEmpty(dieFbx))
        {
            var die = sm.AddState("Die");
            die.motion = LoadClip(dieFbx);
            var toDie = sm.AddAnyStateTransition(die);
            toDie.AddCondition(AnimatorConditionMode.If, 0, "Die");
            toDie.hasExitTime = false; toDie.duration = 0.1f; toDie.canTransitionToSelf = false;
        }
        EditorUtility.SetDirty(ctrl);
        Debug.Log($"[AnimalAssembly] controller {path}: Move={loco.motion!=null}");
        return ctrl;
    }

    static void BuildCharacterPrefab(string prefabPath, string modelFbx, string materialPath, string controllerPath, float scale)
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelFbx);
        if (model == null) { Debug.LogError("[AnimalAssembly] no model " + modelFbx); return; }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(model);
        go.transform.localScale = Vector3.one * scale;

        var anim = go.GetComponent<Animator>();
        if (anim == null) anim = go.AddComponent<Animator>();
        anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        var av = LoadAvatar(modelFbx); if (av != null) anim.avatar = av;
        anim.applyRootMotion = false;

        var mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (mat != null) foreach (var smr in go.GetComponentsInChildren<SkinnedMeshRenderer>()) smr.sharedMaterial = mat;

        bool hasAv = anim.avatar != null;
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        UnityEngine.Object.DestroyImmediate(go);
        Debug.Log($"[AnimalAssembly] char prefab {prefabPath}: mat={mat!=null} avatar={hasAv}");
    }

    static void BuildPropPrefab(string prefabPath, string modelFbx, string materialPath)
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelFbx);
        if (model == null) { Debug.LogError("[AnimalAssembly] no prop " + modelFbx); return; }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(model);
        var mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (mat != null) foreach (var mr in go.GetComponentsInChildren<MeshRenderer>()) mr.sharedMaterial = mat;
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        UnityEngine.Object.DestroyImmediate(go);
        Debug.Log($"[AnimalAssembly] prop prefab {prefabPath}: mat={mat!=null}");
    }

    static void EnsureFolder(string assetFolder)
    {
        var trimmed = assetFolder.TrimEnd('/');
        if (!AssetDatabase.IsValidFolder(trimmed))
        {
            var parent = trimmed.Substring(0, trimmed.LastIndexOf('/'));
            var leaf = trimmed.Substring(trimmed.LastIndexOf('/') + 1);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
