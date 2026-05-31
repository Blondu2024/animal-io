using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildPanelWave
{
    [MenuItem("MCP/Build Panel Wave")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Wave] stop play first"); return; }
        var corridor = GameObject.Find("Corridor");
        if (corridor == null) { Debug.LogError("[Wave] Build the Hook corridor first"); return; }

        foreach (var n in new[] { "GapWall", "GapL", "GapR", "PanelWave", "Panel_Gap", "Panel_Jump", "Panel_Dodge", "Panel_Slide" })
        {
            var g = GameObject.Find(n);
            if (g != null) Object.DestroyImmediate(g);
        }

        // Spread out so the player sees all three coming in the distance
        var p0 = MakePanel("Panel_Gap", MovingPanel.Mode.Gap, new Color(0.5f, 0.2f, 0.2f), corridor.transform, 30f);
        var p1 = MakePanel("Panel_Jump", MovingPanel.Mode.Jump, new Color(0.2f, 0.4f, 0.5f), corridor.transform, 48f);
        var p2 = MakePanel("Panel_Slide", MovingPanel.Mode.Slide, new Color(0.5f, 0.4f, 0.15f), corridor.transform, 66f);

        var waveGO = new GameObject("PanelWave");
        waveGO.transform.SetParent(corridor.transform);
        var wave = waveGO.AddComponent<PanelWave>();
        wave.panels = new[] { p0, p1, p2 };
        wave.startDelay = 3f;

        EditorSceneManager.MarkSceneDirty(corridor.scene);
        EditorSceneManager.SaveScene(corridor.scene);
        Debug.Log("[Wave] DONE. 3-panel wave (Gap / Jump / Dodge) built.");
    }

    static MovingPanel MakePanel(string name, MovingPanel.Mode mode, Color col, Transform parent, float z)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = new Vector3(0f, 0f, z);

        var a = Blocker(name + "_A", go.transform, col);
        var b = Blocker(name + "_B", go.transform, col);

        var mp = go.AddComponent<MovingPanel>();
        mp.mode = mode; mp.a = a.transform; mp.b = b.transform; mp.speed = 4f;
        return mp;
    }

    static GameObject Blocker(string name, Transform parent, Color col)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name; go.transform.SetParent(parent);
        go.transform.localPosition = new Vector3(0f, 1.7f, 0f);
        go.transform.localScale = new Vector3(3f, 3.4f, 0.5f);
        var sh = Shader.Find("Universal Render Pipeline/Lit"); if (sh == null) sh = Shader.Find("Standard");
        go.GetComponent<Renderer>().sharedMaterial = new Material(sh) { color = col };
        go.GetComponent<Collider>().isTrigger = true;
        go.AddComponent<KillOnTouch>();
        go.SetActive(false);
        return go;
    }
}
