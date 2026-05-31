using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildCombatGate
{
    [MenuItem("MCP/Build Combat Gate")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Gate] stop play first"); return; }
        var player = GameObject.Find("PlayerArmature");
        if (player == null) { Debug.LogError("[Gate] no player"); return; }
        var corridor = GameObject.Find("Corridor");
        if (corridor == null) { Debug.LogError("[Gate] no corridor"); return; }

        if (player.GetComponent<PlayerCombat>() == null) player.AddComponent<PlayerCombat>();

        // Barrel STAYS (first obstacle). Find via component so inactive ones are found too.
        var barrels = Object.FindObjectsByType<RollingHazard>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        RollingHazard barrel = barrels.Length > 0 ? barrels[0] : null;
        if (barrel != null) barrel.gameObject.SetActive(true);

        // Make sure the hook still drives the barrel
        foreach (var hs in Object.FindObjectsByType<HookSequence>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            hs.gameObject.SetActive(true);
            if (barrel != null) hs.barrel = barrel;
        }

        // Panels: Gap active (used by gate), Jump/Slide off
        MovingPanel gapPanel = null;
        foreach (var mp in Object.FindObjectsByType<MovingPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            bool isGap = mp.mode == MovingPanel.Mode.Gap;
            mp.gameObject.SetActive(isGap);
            if (isGap) gapPanel = mp;
        }

        // Disable the auto wave launcher
        foreach (var pw in Object.FindObjectsByType<PanelWave>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            pw.gameObject.SetActive(false);

        // Clean old gate stuff
        foreach (var n in new[] { "Barrier", "BackFloor", "CombatGate", "GateBarrier" })
        {
            var g = GameObject.Find(n);
            if (g != null) Object.DestroyImmediate(g);
        }
        foreach (var z in Object.FindObjectsByType<Zombie>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            Object.DestroyImmediate(z.gameObject);

        var gateGO = new GameObject("CombatGate");
        gateGO.transform.SetParent(corridor.transform);
        var gate = gateGO.AddComponent<CombatGate>();
        gate.triggerZ = 12f;
        gate.panel = gapPanel;

        EditorSceneManager.MarkSceneDirty(player.scene);
        EditorSceneManager.SaveScene(player.scene);
        Debug.Log("[Gate] DONE. barrel=" + (barrel != null) + " gapPanel=" + (gapPanel != null));
    }
}
