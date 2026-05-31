using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class ArcherControllerBuilder
{
    [MenuItem("MCP/Build Archer Controller")]
    public static void Build()
    {
        const string glb = "Assets/Models/character.glb";
        AnimationClip idle = null, walk = null, attack = null;
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(glb))
        {
            if (o is AnimationClip c)
            {
                if (c.name == "CharacterArmature|Idle") idle = c;
                else if (c.name == "CharacterArmature|Walk") walk = c;
                else if (c.name == "CharacterArmature|Attacking_Idle") attack = c;
            }
        }

        const string path = "Assets/Animators/Archer.controller";
        AssetDatabase.DeleteAsset(path);
        var ac = AnimatorController.CreateAnimatorControllerAtPath(path);
        var sm = ac.layers[0].stateMachine;

        var sIdle = sm.AddState("Idle");
        sIdle.motion = idle;
        var sWalk = sm.AddState("Walk");
        sWalk.motion = walk;
        var sAttack = sm.AddState("Attack");
        sAttack.motion = attack;
        sm.defaultState = sIdle;

        EditorUtility.SetDirty(ac);
        AssetDatabase.SaveAssets();

        // Assign to the player's model in the open scene.
        var go = GameObject.Find("ArcherModel");
        if (go != null)
        {
            var an = go.GetComponent<Animator>();
            if (an != null)
            {
                an.runtimeAnimatorController = ac;
                EditorUtility.SetDirty(an);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(go.scene);
            }
        }

        Debug.Log("Archer controller built. idle=" + (idle != null) + " walk=" + (walk != null) + " attack=" + (attack != null) + " assigned=" + (go != null));
    }
}
