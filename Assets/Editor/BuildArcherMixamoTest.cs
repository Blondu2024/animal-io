using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildArcherMixamoTest
{
    static readonly string[] Order =
    {
        "Assets/Mixamo/Running.fbx",
        "Assets/Mixamo/Running Jump.fbx",
        "Assets/Mixamo/Running Slide.fbx",
        "Assets/Mixamo/Running Crawl.fbx",
        "Assets/Mixamo/Climbing Ladder.fbx",
    };

    [MenuItem("MCP/Build Archer Mixamo Test")]
    public static void Build()
    {
        // 1) Report avatar validity + collect one clip per file
        var clips = new List<AnimationClip>();
        foreach (var path in Order)
        {
            var avatar = AssetDatabase.LoadAssetAtPath<Avatar>(path);
            Debug.Log("[Test] " + System.IO.Path.GetFileName(path) + " avatar valid=" +
                      (avatar != null && avatar.isValid) + " human=" + (avatar != null && avatar.isHuman));

            AnimationClip clip = null;
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                var c = o as AnimationClip;
                if (c != null && !c.name.StartsWith("__preview")) { clip = c; break; }
            }
            if (clip != null) clips.Add(clip);
            else Debug.LogWarning("[Test] No clip in " + path);
        }
        if (clips.Count == 0) { Debug.LogError("[Test] No clips found."); return; }

        // 2) Build animator controller that auto-cycles through all clips
        var ctrlPath = "Assets/Mixamo/ArcherTest.controller";
        AssetDatabase.DeleteAsset(ctrlPath);
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
        var sm = ctrl.layers[0].stateMachine;
        var labels = new[] { "Running", "Jump", "Slide", "Crawl", "Climbing" };
        var states = new List<AnimatorState>();
        for (int i = 0; i < clips.Count; i++)
        {
            var st = sm.AddState(i < labels.Length ? labels[i] : ("Clip" + i));
            st.motion = clips[i];
            states.Add(st);
        }
        sm.defaultState = states[0];
        for (int i = 0; i < states.Count; i++)
        {
            var next = states[(i + 1) % states.Count];
            var tr = states[i].AddTransition(next);
            tr.hasExitTime = true;
            tr.exitTime = 0.9f;
            tr.duration = 0.15f;
            tr.hasFixedDuration = true;
        }
        AssetDatabase.SaveAssets();

        // 3) New scene with ground, light, camera
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(5, 1, 5);

        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

        // 4) Instantiate archer character
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(Order[0]);
        var archer = (GameObject)PrefabUtility.InstantiatePrefab(model);
        archer.name = "Archer";
        archer.transform.position = Vector3.zero;

        var animator = archer.GetComponent<Animator>();
        if (animator == null) animator = archer.AddComponent<Animator>();
        animator.runtimeAnimatorController = ctrl;
        animator.applyRootMotion = false;

        // make it visible with a basic URP material (Mixamo FBX has no textures)
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        var mat = new Material(shader);
        mat.color = new Color(0.7f, 0.55f, 0.4f);
        foreach (var smr in archer.GetComponentsInChildren<SkinnedMeshRenderer>())
            smr.sharedMaterial = mat;

        // 5) Frame camera on the character bounds
        var bounds = new Bounds(archer.transform.position, Vector3.one);
        var rends = archer.GetComponentsInChildren<Renderer>();
        if (rends.Length > 0)
        {
            bounds = rends[0].bounds;
            foreach (var r in rends) bounds.Encapsulate(r.bounds);
        }
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        float dist = bounds.size.magnitude * 1.4f + 1f;
        camGO.transform.position = bounds.center + new Vector3(0, bounds.size.y * 0.15f, dist);
        camGO.transform.LookAt(bounds.center);

        var scenePath = "Assets/Scenes/ArcherMixamoTest.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[Test] DONE. Scene saved -> " + scenePath + " | clips=" + clips.Count +
                  " | bounds size=" + bounds.size);
    }
}
