using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class AddGapWall
{
    [MenuItem("MCP/Add Gap Wall")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Gap] stop play first"); return; }
        var corridor = GameObject.Find("Corridor");
        if (corridor == null) { Debug.LogError("[Gap] Build the Hook corridor first"); return; }

        foreach (var n in new[] { "GapWall", "GapL", "GapR" })
        {
            var g = GameObject.Find(n);
            if (g != null) Object.DestroyImmediate(g);
        }

        var mat = Mat(new Color(0.5f, 0.2f, 0.2f));

        // Moving wall starts far down the corridor
        var wallGO = new GameObject("GapWall");
        wallGO.transform.SetParent(corridor.transform);
        wallGO.transform.position = new Vector3(0f, 0f, 30f);

        var left = GameObject.CreatePrimitive(PrimitiveType.Cube);
        left.name = "GapL"; left.transform.SetParent(wallGO.transform);
        left.transform.localPosition = new Vector3(-1.5f, 1.7f, 0f);
        left.transform.localScale = new Vector3(3f, 3.4f, 0.5f);
        left.GetComponent<Renderer>().sharedMaterial = mat;
        left.GetComponent<Collider>().isTrigger = true;
        left.AddComponent<KillOnTouch>();

        var right = GameObject.CreatePrimitive(PrimitiveType.Cube);
        right.name = "GapR"; right.transform.SetParent(wallGO.transform);
        right.transform.localPosition = new Vector3(1.5f, 1.7f, 0f);
        right.transform.localScale = new Vector3(3f, 3.4f, 0.5f);
        right.GetComponent<Renderer>().sharedMaterial = mat;
        right.GetComponent<Collider>().isTrigger = true;
        right.AddComponent<KillOnTouch>();

        var rg = wallGO.AddComponent<RandomGapWall>();
        rg.leftWall = left.transform;
        rg.rightWall = right.transform;

        EditorSceneManager.MarkSceneDirty(corridor.scene);
        EditorSceneManager.SaveScene(corridor.scene);
        Debug.Log("[Gap] DONE. Moving gap wall (shifts every 2s) added.");
    }

    static Material Mat(Color c)
    {
        var sh = Shader.Find("Universal Render Pipeline/Lit"); if (sh == null) sh = Shader.Find("Standard");
        return new Material(sh) { color = c };
    }
}
