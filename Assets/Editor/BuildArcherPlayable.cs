using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildArcherPlayable
{
    // file -> param mapping
    static readonly string[] Order =
    {
        "Assets/Mixamo/Running.fbx",        // 0 locomotion
        "Assets/Mixamo/Running Jump.fbx",   // 1 jump
        "Assets/Mixamo/Running Slide.fbx",  // 2 slide
        "Assets/Mixamo/Running Crawl.fbx",  // 3 crawl
        "Assets/Mixamo/Climbing Ladder.fbx" // 4 climb
    };

    static AnimationClip ClipOf(string path)
    {
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            var c = o as AnimationClip;
            if (c != null && !c.name.StartsWith("__preview")) return c;
        }
        return null;
    }

    [MenuItem("MCP/Build Archer Playable")]
    public static void Build()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Playable] Stop play mode first.");
            return;
        }

        var clips = new List<AnimationClip>();
        foreach (var p in Order) clips.Add(ClipOf(p));
        if (clips[0] == null) { Debug.LogError("[Playable] Running clip missing."); return; }

        // --- Animator controller with input params ---
        var ctrlPath = "Assets/Mixamo/ArcherPlayable.controller";
        AssetDatabase.DeleteAsset(ctrlPath);
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
        ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
        ctrl.AddParameter("JumpTrig", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Slide", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Crawl", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Climb", AnimatorControllerParameterType.Bool);

        var sm = ctrl.layers[0].stateMachine;
        var locomotion = sm.AddState("Locomotion"); locomotion.motion = clips[0];
        var jump = sm.AddState("Jump"); jump.motion = clips[1];
        var slide = sm.AddState("Slide"); slide.motion = clips[2];
        var crawl = sm.AddState("Crawl"); crawl.motion = clips[3];
        var climb = sm.AddState("Climb"); climb.motion = clips[4];
        sm.defaultState = locomotion;

        // Jump (AnyState trigger -> back by exit time)
        var tJump = sm.AddAnyStateTransition(jump);
        tJump.AddCondition(AnimatorConditionMode.If, 0, "JumpTrig");
        tJump.duration = 0.1f; tJump.canTransitionToSelf = false;
        var tJumpBack = jump.AddTransition(locomotion);
        tJumpBack.hasExitTime = true; tJumpBack.exitTime = 0.8f; tJumpBack.duration = 0.15f;

        // Slide
        var tSlide = sm.AddAnyStateTransition(slide);
        tSlide.AddCondition(AnimatorConditionMode.If, 0, "Slide");
        tSlide.duration = 0.1f; tSlide.canTransitionToSelf = false;
        var tSlideBack = slide.AddTransition(locomotion);
        tSlideBack.hasExitTime = true; tSlideBack.exitTime = 0.85f; tSlideBack.duration = 0.15f;

        // Crawl (bool hold)
        var tCrawl = locomotion.AddTransition(crawl);
        tCrawl.AddCondition(AnimatorConditionMode.If, 0, "Crawl"); tCrawl.hasExitTime = false; tCrawl.duration = 0.15f;
        var tCrawlBack = crawl.AddTransition(locomotion);
        tCrawlBack.AddCondition(AnimatorConditionMode.IfNot, 0, "Crawl"); tCrawlBack.hasExitTime = false; tCrawlBack.duration = 0.15f;

        // Climb (bool hold)
        var tClimb = locomotion.AddTransition(climb);
        tClimb.AddCondition(AnimatorConditionMode.If, 0, "Climb"); tClimb.hasExitTime = false; tClimb.duration = 0.15f;
        var tClimbBack = climb.AddTransition(locomotion);
        tClimbBack.AddCondition(AnimatorConditionMode.IfNot, 0, "Climb"); tClimbBack.hasExitTime = false; tClimbBack.duration = 0.15f;

        AssetDatabase.SaveAssets();

        // --- Scene ---
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(10, 1, 10);

        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional; light.intensity = 1.1f;
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

        var model = AssetDatabase.LoadAssetAtPath<GameObject>(Order[0]);
        var player = (GameObject)PrefabUtility.InstantiatePrefab(model);
        player.name = "Player";
        player.transform.position = Vector3.zero;

        var animator = player.GetComponent<Animator>();
        if (animator == null) animator = player.AddComponent<Animator>();
        animator.runtimeAnimatorController = ctrl;
        animator.applyRootMotion = false;

        var charCtrl = player.AddComponent<CharacterController>();
        charCtrl.height = 1.8f; charCtrl.radius = 0.3f; charCtrl.center = new Vector3(0, 0.95f, 0);

        player.AddComponent<ArcherController>();

        // Fresh visible material (texture is cosmetic, added later)
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        var mat = new Material(shader);
        mat.color = new Color(0.7f, 0.55f, 0.4f);
        foreach (var smr in player.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            smr.sharedMaterial = mat;
            smr.updateWhenOffscreen = true; // animation moves mesh out of static bounds -> would get culled
        }

        // Camera with third-person orbit
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        camGO.AddComponent<Camera>();
        var tpc = camGO.AddComponent<TPOrbitCamera>();
        tpc.target = player.transform;
        camGO.transform.position = new Vector3(0, 1.6f, -4.5f);
        camGO.transform.LookAt(player.transform.position + Vector3.up * 1.2f);

        var scenePath = "Assets/Scenes/ArcherPlayable.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[Playable] DONE -> " + scenePath);
    }
}
