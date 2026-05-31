using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class AddCrawlClimbAnims
{
    static AnimationClip ClipOf(string path)
    {
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            var c = o as AnimationClip;
            if (c != null && !c.name.StartsWith("__preview")) return c;
        }
        return null;
    }

    [MenuItem("MCP/Add Crawl Climb Anims")]
    public static void Apply()
    {
        var player = GameObject.Find("PlayerArmature");
        if (player == null) { Debug.LogError("[Anims] PlayerArmature not found"); return; }
        var animator = player.GetComponent<Animator>();
        var ctrl = animator != null ? animator.runtimeAnimatorController as AnimatorController : null;
        if (ctrl == null) { Debug.LogError("[Anims] No AnimatorController on PlayerArmature"); return; }

        EnsureBool(ctrl, "Crawl");
        EnsureBool(ctrl, "Climb");

        var crawlClip = ClipOf("Assets/Mixamo/Running Crawl.fbx");
        var climbClip = ClipOf("Assets/Mixamo/Climbing Ladder.fbx");
        if (crawlClip == null || climbClip == null) { Debug.LogError("[Anims] Missing Mixamo clips"); return; }

        var sm = ctrl.layers[0].stateMachine;
        var def = sm.defaultState;

        var crawlState = FindOrAddState(sm, "CrawlState"); crawlState.motion = crawlClip;
        // Crawl animation plays only while actually moving (speed driven by a parameter)
        EnsureFloat(ctrl, "CrawlSpeedMult");
        crawlState.speedParameterActive = true;
        crawlState.speedParameter = "CrawlSpeedMult";
        var climbState = FindOrAddState(sm, "ClimbState"); climbState.motion = climbClip;

        AddAnyState(sm, crawlState, "Crawl");
        AddBackToDefault(crawlState, def, "Crawl");
        AddAnyState(sm, climbState, "Climb");
        AddBackToDefault(climbState, def, "Climb");

        // Slide (Temple-Run style, feet-first) on a trigger
        EnsureTrigger(ctrl, "SlideTrig");
        var slideClip = ClipOf("Assets/Mixamo/RunningSlide2.fbx");
        if (slideClip != null)
        {
            var slideState = FindOrAddState(sm, "SlideState"); slideState.motion = slideClip;
            AddAnyState(sm, slideState, "SlideTrig");
            AddBackByExitTime(slideState, def);
        }
        else Debug.LogWarning("[Anims] Running Slide clip missing");

        AssetDatabase.SaveAssets();
        Debug.Log("[Anims] DONE -> " + AssetDatabase.GetAssetPath(ctrl));
    }

    static void EnsureBool(AnimatorController c, string name)
    {
        foreach (var p in c.parameters) if (p.name == name) return;
        c.AddParameter(name, AnimatorControllerParameterType.Bool);
    }

    static void EnsureTrigger(AnimatorController c, string name)
    {
        foreach (var p in c.parameters) if (p.name == name) return;
        c.AddParameter(name, AnimatorControllerParameterType.Trigger);
    }

    static void EnsureFloat(AnimatorController c, string name)
    {
        foreach (var p in c.parameters) if (p.name == name) return;
        c.AddParameter(name, AnimatorControllerParameterType.Float);
    }

    static void AddBackByExitTime(AnimatorState from, AnimatorState def)
    {
        if (def == null) return;
        foreach (var t in from.transitions) if (t.destinationState == def) return;
        var tr = from.AddTransition(def);
        tr.hasExitTime = true; tr.exitTime = 0.85f; tr.duration = 0.15f;
    }

    static AnimatorState FindOrAddState(AnimatorStateMachine sm, string name)
    {
        foreach (var cs in sm.states) if (cs.state.name == name) return cs.state;
        return sm.AddState(name);
    }

    static void AddAnyState(AnimatorStateMachine sm, AnimatorState target, string boolParam)
    {
        foreach (var t in sm.anyStateTransitions) if (t.destinationState == target) return;
        var tr = sm.AddAnyStateTransition(target);
        tr.AddCondition(AnimatorConditionMode.If, 0, boolParam);
        tr.duration = 0.12f; tr.hasExitTime = false; tr.canTransitionToSelf = false;
    }

    static void AddBackToDefault(AnimatorState from, AnimatorState def, string boolParam)
    {
        if (def == null) return;
        foreach (var t in from.transitions) if (t.destinationState == def) return;
        var tr = from.AddTransition(def);
        tr.AddCondition(AnimatorConditionMode.IfNot, 0, boolParam);
        tr.duration = 0.15f; tr.hasExitTime = false;
    }
}
