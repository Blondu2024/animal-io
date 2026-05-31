using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SetupFollowCamera
{
    [MenuItem("MCP/Setup Follow Camera")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Cam] Stop play mode first."); return; }

        var player = GameObject.Find("PlayerArmature");
        if (player == null) { Debug.LogError("[Cam] PlayerArmature not found"); return; }

        var camGO = GameObject.Find("MainCamera");
        if (camGO == null && Camera.main != null) camGO = Camera.main.gameObject;
        if (camGO == null) { Debug.LogError("[Cam] MainCamera not found"); return; }

        // Disable Cinemachine (mouse-driven) so our auto-follow drives the transform
        var brain = camGO.GetComponent("CinemachineBrain") as Behaviour;
        if (brain != null) brain.enabled = false;
        var pfc = GameObject.Find("PlayerFollowCamera");
        if (pfc != null) pfc.SetActive(false);

        // Add / configure auto-follow camera
        var fc = camGO.GetComponent<FollowCamera>();
        if (fc == null) fc = camGO.AddComponent<FollowCamera>();
        fc.target = player.transform;
        fc.distance = 6f;
        fc.lookHeight = 1.3f;
        fc.yawDamp = 4f;
        fc.pitch = 18f; // lower angle so you see far down the corridor
        var cam = camGO.GetComponent<Camera>();
        if (cam != null) cam.fieldOfView = 68f; // wider view to spot panels in the distance

        EditorSceneManager.MarkSceneDirty(player.scene);
        EditorSceneManager.SaveScene(player.scene);
        Debug.Log("[Cam] DONE. Auto-follow camera set up (Cinemachine disabled).");
    }
}
