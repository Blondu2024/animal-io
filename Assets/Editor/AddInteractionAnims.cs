using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class AddInteractionAnims
{
    // one-shot (trigger): path, trigger, state
    static readonly string[] OnePaths = {
        "Assets/Mixamo/Opening.fbx",
        "Assets/Mixamo/Picking Up.fbx",
        "Assets/Mixamo/Button Pushing.fbx",
        "Assets/Mixamo/Victory.fbx",
    };
    static readonly string[] OneTrigs = { "OpenTrig", "PickTrig", "ButtonTrig", "VictoryTrig" };
    static readonly string[] OneStates = { "OpenState", "PickState", "ButtonState", "VictoryState" };

    static AnimationClip ClipOf(string path)
    {
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            var c = o as AnimationClip;
            if (c != null && !c.name.StartsWith("__preview")) return c;
        }
        return null;
    }

    [MenuItem("MCP/Add Interaction Anims")]
    public static void Apply()
    {
        var player = GameObject.Find("PlayerArmature");
        if (player == null) { Debug.LogError("[IAnims] PlayerArmature not found"); return; }
        var animator = player.GetComponent<Animator>();
        var ctrl = animator != null ? animator.runtimeAnimatorController as AnimatorController : null;
        if (ctrl == null) { Debug.LogError("[IAnims] no controller"); return; }
        var sm = ctrl.layers[0].stateMachine;
        var def = sm.defaultState;

        // One-shot interactions (trigger -> play once 2x -> back)
        for (int i = 0; i < OnePaths.Length; i++)
        {
            var clip = ClipOf(OnePaths[i]);
            if (clip == null) { Debug.LogWarning("[IAnims] missing: " + OnePaths[i]); continue; }
            EnsureTrigger(ctrl, OneTrigs[i]);
            var st = FindOrAddState(sm, OneStates[i]);
            st.motion = clip; st.speed = 2f;
            AddAnyTrigger(sm, st, OneTrigs[i]);
            AddBackByExitTime(st, def);
        }

        // Rope = HOLD (bool, looping while held)
        var pullClip = ClipOf("Assets/Mixamo/Pulling A Rope.fbx");
        if (pullClip != null)
        {
            EnsureBool(ctrl, "Pulling");
            var pull = FindOrAddState(sm, "PullState");
            pull.motion = pullClip; pull.speed = 2f;
            // Remove any stale transitions to/from PullState (old trigger version)
            foreach (var t in sm.anyStateTransitions)
                if (t.destinationState == pull) sm.RemoveAnyStateTransition(t);
            foreach (var t in pull.transitions)
                pull.RemoveTransition(t);
            // locomotion -> pull while Pulling==true ; pull -> locomotion when false
            AddBoolIn(sm, pull, "Pulling");
            AddBoolOut(pull, def, "Pulling");
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[IAnims] DONE (one-shot triggers + rope hold)");
    }

    static void EnsureTrigger(AnimatorController c, string name)
    { foreach (var p in c.parameters) if (p.name == name) return; c.AddParameter(name, AnimatorControllerParameterType.Trigger); }

    static void EnsureBool(AnimatorController c, string name)
    { foreach (var p in c.parameters) if (p.name == name) return; c.AddParameter(name, AnimatorControllerParameterType.Bool); }

    static AnimatorState FindOrAddState(AnimatorStateMachine sm, string name)
    { foreach (var cs in sm.states) if (cs.state.name == name) return cs.state; return sm.AddState(name); }

    static void AddAnyTrigger(AnimatorStateMachine sm, AnimatorState target, string trig)
    {
        foreach (var t in sm.anyStateTransitions) if (t.destinationState == target) return;
        var tr = sm.AddAnyStateTransition(target);
        tr.AddCondition(AnimatorConditionMode.If, 0, trig);
        tr.duration = 0.1f; tr.hasExitTime = false; tr.canTransitionToSelf = false;
    }

    static void AddBoolIn(AnimatorStateMachine sm, AnimatorState target, string boolName)
    {
        foreach (var t in sm.anyStateTransitions) if (t.destinationState == target) return;
        var tr = sm.AddAnyStateTransition(target);
        tr.AddCondition(AnimatorConditionMode.If, 0, boolName);
        tr.duration = 0.12f; tr.hasExitTime = false; tr.canTransitionToSelf = false;
    }

    static void AddBoolOut(AnimatorState from, AnimatorState def, string boolName)
    {
        if (def == null) return;
        foreach (var t in from.transitions) if (t.destinationState == def) return;
        var tr = from.AddTransition(def);
        tr.AddCondition(AnimatorConditionMode.IfNot, 0, boolName);
        tr.duration = 0.12f; tr.hasExitTime = false;
    }

    static void AddBackByExitTime(AnimatorState from, AnimatorState def)
    {
        if (def == null) return;
        foreach (var t in from.transitions) if (t.destinationState == def) return;
        var tr = from.AddTransition(def);
        tr.hasExitTime = true; tr.exitTime = 0.85f; tr.duration = 0.15f;
    }
}
