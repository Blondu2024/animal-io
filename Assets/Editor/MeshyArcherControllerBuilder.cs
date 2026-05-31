using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class MeshyArcherControllerBuilder
{
    const string CtrlPath = "Assets/Animators/MeshyArcher.controller";

    [MenuItem("MCP/Build Meshy Archer Controller")]
    public static void Build()
    {
        var dir = Path.GetDirectoryName(CtrlPath);
        if (!AssetDatabase.IsValidFolder(dir))
        {
            Directory.CreateDirectory(dir);
            AssetDatabase.Refresh();
        }

        AssetDatabase.DeleteAsset(CtrlPath);
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(CtrlPath);

        ctrl.parameters = new[] {
            new AnimatorControllerParameter { name = "Speed",     type = AnimatorControllerParameterType.Float },
            new AnimatorControllerParameter { name = "Grounded",  type = AnimatorControllerParameterType.Bool, defaultBool = true },
            new AnimatorControllerParameter { name = "Sprinting", type = AnimatorControllerParameterType.Bool },
            new AnimatorControllerParameter { name = "Crawling",  type = AnimatorControllerParameterType.Bool },
            new AnimatorControllerParameter { name = "Climbing",  type = AnimatorControllerParameterType.Bool },
            new AnimatorControllerParameter { name = "Rolling",   type = AnimatorControllerParameterType.Bool },
            new AnimatorControllerParameter { name = "Jump",      type = AnimatorControllerParameterType.Trigger },
        };

        var sm = ctrl.layers[0].stateMachine;

        var idle = AddState(sm, "Idle", Load("archer_idle.glb"));
        sm.defaultState = idle;
        var walk = AddState(sm, "Walk", Load("archer_walk.glb"));
        var run = AddState(sm, "Run", Load("archer_run.glb"));
        var jump = AddState(sm, "Jump", Load("archer_jump.glb"));
        var roll = AddState(sm, "Roll", Load("archer_roll.glb"));
        roll.speed = 4f;
        var crawl = AddState(sm, "Crawl", Load("archer_crawl.glb"));
        var climb = AddState(sm, "Climb", Load("archer_climb.glb"));

        // Speed-based transitions between Idle/Walk/Run
        AddTransition(idle, walk, t => t.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed"));
        AddTransition(walk, idle, t => t.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed"));
        AddTransition(walk, run, t => { t.AddCondition(AnimatorConditionMode.If, 0, "Sprinting"); t.AddCondition(AnimatorConditionMode.Greater, 5f, "Speed"); });
        AddTransition(run, walk, t => t.AddCondition(AnimatorConditionMode.IfNot, 0, "Sprinting"));
        AddTransition(run, idle, t => t.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed"));

        // Jump trigger from any state
        AddAnyTransition(sm, jump, t => { t.AddCondition(AnimatorConditionMode.If, 0, "Jump"); }, true);
        // Jump -> idle when back on ground
        AddTransition(jump, idle, t => t.AddCondition(AnimatorConditionMode.If, 0, "Grounded"), 0.12f);

        // Roll
        AddAnyTransition(sm, roll, t => t.AddCondition(AnimatorConditionMode.If, 0, "Rolling"), false);
        AddTransition(roll, idle, t => t.AddCondition(AnimatorConditionMode.IfNot, 0, "Rolling"));

        // Crawl
        AddAnyTransition(sm, crawl, t => t.AddCondition(AnimatorConditionMode.If, 0, "Crawling"), false);
        AddTransition(crawl, idle, t => t.AddCondition(AnimatorConditionMode.IfNot, 0, "Crawling"));

        // Climb
        AddAnyTransition(sm, climb, t => t.AddCondition(AnimatorConditionMode.If, 0, "Climbing"), false);
        AddTransition(climb, idle, t => t.AddCondition(AnimatorConditionMode.IfNot, 0, "Climbing"));

        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[MeshyArcher] Controller built at {CtrlPath} with 7 states and 7 parameters.");
    }

    static AnimatorState AddState(AnimatorStateMachine sm, string name, AnimationClip clip)
    {
        var s = sm.AddState(name);
        s.motion = clip;
        return s;
    }

    static void AddTransition(AnimatorState from, AnimatorState to, System.Action<AnimatorStateTransition> cfg, float exitTime = 0f)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration = 0.12f;
        t.canTransitionToSelf = false;
        cfg(t);
    }

    static void AddAnyTransition(AnimatorStateMachine sm, AnimatorState to, System.Action<AnimatorStateTransition> cfg, bool isTrigger)
    {
        var t = sm.AddAnyStateTransition(to);
        t.hasExitTime = false;
        t.duration = 0.10f;
        t.canTransitionToSelf = false;
        cfg(t);
    }

    static AnimationClip Load(string fileName)
    {
        var path = $"Assets/Models/{fileName}";
        var assets = AssetDatabase.LoadAllAssetsAtPath(path);
        return assets.OfType<AnimationClip>()
                     .FirstOrDefault(c => !c.name.StartsWith("__preview__"));
    }

    [MenuItem("MCP/Rebind Meshy Archer Animator")]
    public static void Rebind()
    {
        var go = GameObject.Find("MeshyArcher");
        if (go == null) { Debug.LogError("[MeshyArcher] MeshyArcher GameObject not found in scene."); return; }
        var anim = go.GetComponent<Animator>();
        if (anim == null) { Debug.LogError("[MeshyArcher] Animator missing on MeshyArcher."); return; }
        var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CtrlPath);
        anim.runtimeAnimatorController = null;
        anim.runtimeAnimatorController = ctrl;
        anim.Rebind();
        EditorUtility.SetDirty(go);
        EditorSceneManagerMark();
        var ac = ctrl as AnimatorController;
        int pcount = ac != null ? ac.parameters.Length : -1;
        Debug.Log($"[MeshyArcher] Rebound Animator to {CtrlPath} (params={pcount}).");
    }

    static void EditorSceneManagerMark()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
    }

    [MenuItem("MCP/List Meshy Archer Clips")]
    public static void ListClips()
    {
        foreach (var name in new[] { "archer_idle", "archer_walk", "archer_run", "archer_jump", "archer_roll", "archer_crawl", "archer_climb", "archer_attack", "archer_rigged" })
        {
            var full = $"Assets/Models/{name}.glb";
            var assets = AssetDatabase.LoadAllAssetsAtPath(full);
            var clips = assets.OfType<AnimationClip>().ToArray();
            Debug.Log($"[Clips/{name}] {clips.Length} clips: {string.Join(", ", clips.Select(c => $"'{c.name}' ({c.length:F2}s loop={c.isLooping})"))}");
        }
    }
}
