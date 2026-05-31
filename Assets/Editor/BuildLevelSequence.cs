using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildLevelSequence
{
    [MenuItem("MCP/Build Level Sequence")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Seq] stop play first"); return; }
        var player = GameObject.Find("PlayerArmature");
        if (player == null) { Debug.LogError("[Seq] no player"); return; }
        var corridor = GameObject.Find("Corridor");
        if (corridor == null) { Debug.LogError("[Seq] no corridor"); return; }

        if (player.GetComponent<PlayerCombat>() == null) player.AddComponent<PlayerCombat>();

        // Barrel
        var barrels = Object.FindObjectsByType<RollingHazard>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        RollingHazard barrel = barrels.Length > 0 ? barrels[0] : null;
        if (barrel != null) barrel.gameObject.SetActive(true);

        // Panels in order Gap, Jump, Slide
        var all = Object.FindObjectsByType<MovingPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        MovingPanel gap = null, jump = null, slide = null;
        foreach (var mp in all)
        {
            mp.gameObject.SetActive(true);
            if (mp.mode == MovingPanel.Mode.Gap) gap = mp;
            else if (mp.mode == MovingPanel.Mode.Jump) jump = mp;
            else if (mp.mode == MovingPanel.Mode.Slide) slide = mp;
        }
        var panels = new List<MovingPanel>();
        if (gap != null) panels.Add(gap);
        if (jump != null) panels.Add(jump);
        if (slide != null) panels.Add(slide);

        // Disable old orchestrators (the sequencer takes over)
        foreach (var hs in Object.FindObjectsByType<HookSequence>(FindObjectsInactive.Include, FindObjectsSortMode.None)) hs.gameObject.SetActive(false);
        foreach (var pw in Object.FindObjectsByType<PanelWave>(FindObjectsInactive.Include, FindObjectsSortMode.None)) pw.gameObject.SetActive(false);
        foreach (var cg in Object.FindObjectsByType<CombatGate>(FindObjectsInactive.Include, FindObjectsSortMode.None)) Object.DestroyImmediate(cg.gameObject);
        foreach (var z in Object.FindObjectsByType<Zombie>(FindObjectsInactive.Include, FindObjectsSortMode.None)) Object.DestroyImmediate(z.gameObject);
        foreach (var n in new[] { "Barrier", "GateBarrier", "BackFloor", "LevelSequencer" })
        {
            var g = GameObject.Find(n);
            if (g != null) Object.DestroyImmediate(g);
        }

        // Lights
        var dir = GameObject.Find("Directional Light");
        var revealGO = GameObject.Find("RevealLight");

        var seqGO = new GameObject("LevelSequencer");
        seqGO.transform.SetParent(corridor.transform);
        var seq = seqGO.AddComponent<LevelSequencer>();
        seq.barrel = barrel;
        seq.panels = panels.ToArray();
        if (dir != null) seq.mainLight = dir.GetComponent<Light>();
        if (revealGO != null) seq.revealLight = revealGO.GetComponent<Light>();

        EditorSceneManager.MarkSceneDirty(player.scene);
        EditorSceneManager.SaveScene(player.scene);
        Debug.Log("[Seq] DONE. barrel=" + (barrel != null) + " panels=" + panels.Count);
    }
}
