using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildKnifeTest
{
    [MenuItem("MCP/Build Knife Test")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Knife] stop play first"); return; }
        var player = GameObject.Find("PlayerArmature");
        if (player == null) { Debug.LogError("[Knife] no player"); return; }

        // Focus on combat: hide the dodge hazards for now
        foreach (var n in new[] { "Barrel", "PanelWave", "Panel_Gap", "Panel_Jump", "Panel_Slide" })
        {
            var g = GameObject.Find(n);
            if (g != null) g.SetActive(false);
        }

        if (player.GetComponent<PlayerCombat>() == null) player.AddComponent<PlayerCombat>();

        foreach (var z in Object.FindObjectsByType<Zombie>(FindObjectsSortMode.None))
            Object.DestroyImmediate(z.gameObject);

        var corridor = GameObject.Find("Corridor");
        var sh = Shader.Find("Universal Render Pipeline/Lit"); if (sh == null) sh = Shader.Find("Standard");
        var mat = new Material(sh) { color = new Color(0.3f, 0.6f, 0.25f) };

        for (int i = 0; i < 3; i++)
        {
            var z = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            z.name = "Zombie" + i;
            if (corridor != null) z.transform.SetParent(corridor.transform);
            z.transform.position = new Vector3((i - 1) * 1.4f, 1f, 9f + i * 2.5f);
            z.GetComponent<Renderer>().sharedMaterial = mat;
            z.AddComponent<Zombie>();
        }

        EditorSceneManager.MarkSceneDirty(player.scene);
        EditorSceneManager.SaveScene(player.scene);
        Debug.Log("[Knife] DONE. 3 zombies + PlayerCombat. Barrel/panels hidden for focus.");
    }
}
