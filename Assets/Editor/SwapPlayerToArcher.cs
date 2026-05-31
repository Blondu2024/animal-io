using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SwapPlayerToArcher
{
    const string ArcherFbx = "Assets/Mixamo/Running.fbx";

    [MenuItem("MCP/Swap Player To Archer")]
    public static void Swap()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Swap] Stop play mode first."); return; }

        var player = GameObject.Find("PlayerArmature");
        if (player == null) { Debug.LogError("[Swap] PlayerArmature not found."); return; }

        var animator = player.GetComponent<Animator>();
        if (animator == null) { Debug.LogError("[Swap] No Animator on PlayerArmature."); return; }

        // 1) Preserve the camera target: reparent PlayerCameraRoot directly under PlayerArmature
        Transform camRoot = FindDeep(player.transform, "PlayerCameraRoot");
        if (camRoot != null && camRoot.parent != player.transform)
            camRoot.SetParent(player.transform, true);

        // 2) Delete old visual/skeleton children (keep PlayerCameraRoot)
        var toDelete = new List<GameObject>();
        foreach (Transform child in player.transform)
            if (child.name != "PlayerCameraRoot") toDelete.Add(child.gameObject);
        foreach (var go in toDelete) Object.DestroyImmediate(go);

        // 3) Instantiate the archer mesh+skeleton under PlayerArmature
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(ArcherFbx);
        if (model == null) { Debug.LogError("[Swap] Archer model missing: " + ArcherFbx); return; }
        var archer = (GameObject)PrefabUtility.InstantiatePrefab(model);
        archer.name = "ArcherGeo";
        archer.transform.SetParent(player.transform, false);
        archer.transform.localPosition = Vector3.zero;
        archer.transform.localRotation = Quaternion.identity;
        archer.transform.localScale = Vector3.one;

        // The instantiated FBX root has its own Animator -> remove it (PlayerArmature drives animation)
        var childAnim = archer.GetComponent<Animator>();
        if (childAnim != null) Object.DestroyImmediate(childAnim);

        // 4) Point PlayerArmature's Animator at the archer Humanoid avatar
        var avatar = AssetDatabase.LoadAssetAtPath<Avatar>(ArcherFbx);
        if (avatar != null && avatar.isValid) animator.avatar = avatar;
        else Debug.LogWarning("[Swap] Archer avatar not found/invalid; animation may not retarget.");

        // 5) Visible material + no culling while animating
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(shader) { color = new Color(0.7f, 0.55f, 0.4f) };
        foreach (var smr in archer.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            smr.sharedMaterial = mat;
            smr.updateWhenOffscreen = true;
        }

        EditorSceneManager.MarkSceneDirty(player.scene);
        EditorSceneManager.SaveScene(player.scene);
        Debug.Log("[Swap] DONE. Archer swapped onto PlayerArmature. avatar=" + (avatar != null && avatar.isValid));
    }

    static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name) return root;
        foreach (Transform c in root)
        {
            var r = FindDeep(c, name);
            if (r != null) return r;
        }
        return null;
    }
}
