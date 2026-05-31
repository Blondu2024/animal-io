using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class AddKnifeAnim
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

    [MenuItem("MCP/Add Knife Anim")]
    public static void Apply()
    {
        var player = GameObject.Find("PlayerArmature");
        if (player == null) { Debug.LogError("[Attack] no player"); return; }
        var ctrl = player.GetComponent<Animator>().runtimeAnimatorController as AnimatorController;
        if (ctrl == null) { Debug.LogError("[Attack] no controller"); return; }

        WireAttack(ctrl, "Assets/Mixamo/BowShoot.fbx", "AttackTrig", "AttackState", 1.5f);

        AssetDatabase.SaveAssets();
        Debug.Log("[Attack] bow shoot wired");
    }

    static void WireAttack(AnimatorController ctrl, string path, string trig, string stateName, float speed)
    {
        var clip = ClipOf(path);
        if (clip == null) { Debug.LogWarning("[Attack] missing " + path); return; }

        bool hasParam = false;
        foreach (var p in ctrl.parameters) if (p.name == trig) hasParam = true;
        if (!hasParam) ctrl.AddParameter(trig, AnimatorControllerParameterType.Trigger);

        var sm = ctrl.layers[0].stateMachine;
        var def = sm.defaultState;

        AnimatorState st = null;
        foreach (var cs in sm.states) if (cs.state.name == stateName) st = cs.state;
        if (st == null) st = sm.AddState(stateName);
        st.motion = clip; st.speed = speed;

        bool hasIn = false;
        foreach (var t in sm.anyStateTransitions) if (t.destinationState == st) hasIn = true;
        if (!hasIn)
        {
            var tr = sm.AddAnyStateTransition(st);
            tr.AddCondition(AnimatorConditionMode.If, 0, trig);
            tr.duration = 0.05f; tr.hasExitTime = false; tr.canTransitionToSelf = false;
        }
        bool hasOut = false;
        foreach (var t in st.transitions) if (t.destinationState == def) hasOut = true;
        if (!hasOut && def != null)
        {
            var tr = st.AddTransition(def);
            tr.hasExitTime = true; tr.exitTime = 0.7f; tr.duration = 0.12f;
        }
    }
}
