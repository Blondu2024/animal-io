using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Places a few heart pickups (life) + a sample super-arrow pickup along the level.
// Idempotent: destroys any existing "Pickups" and rebuilds.
public static class BuildPickups
{
    [MenuItem("MCP/Build Pickups")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Pickups] stop play first"); return; }
        var corridor = GameObject.Find("Corridor");
        if (corridor == null) { Debug.LogError("[Pickups] no Corridor"); return; }

        var old = corridor.transform.Find("Pickups");
        if (old != null) Object.DestroyImmediate(old.gameObject);
        var root = new GameObject("Pickups"); root.transform.SetParent(corridor.transform);

        // Hearts at combat / pre-gauntlet spots
        foreach (var p in new[] { new Vector3(-1.5f, 1.2f, 45f), new Vector3(1.5f, 1.2f, 75f), new Vector3(-1.4f, 1.2f, 130f), new Vector3(0f, 1.2f, 184f) })
            Heart(root.transform, p);

        // One sample super-arrow pickup (the main source is opened chests)
        SuperArrow(root.transform, new Vector3(1.4f, 1.3f, 78f));

        EditorSceneManager.MarkSceneDirty(corridor.scene);
        EditorSceneManager.SaveScene(corridor.scene);
        Debug.Log("[Pickups] DONE. 4 hearts + 1 super-arrow.");
    }

    static void Heart(Transform parent, Vector3 pos)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "HeartPickup"; go.transform.SetParent(parent);
        go.transform.position = pos; go.transform.localScale = Vector3.one * 0.5f;
        go.GetComponent<Renderer>().sharedMaterial = Mat(new Color(1f, 0.15f, 0.15f), new Color(1.3f, 0.1f, 0.1f));
        var pk = go.AddComponent<Pickup>(); pk.kind = Pickup.Kind.Heart;
    }

    static void SuperArrow(Transform parent, Vector3 pos)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "SuperArrowPickup"; go.transform.SetParent(parent);
        go.transform.position = pos; go.transform.localScale = new Vector3(0.18f, 0.18f, 0.9f);
        go.GetComponent<Renderer>().sharedMaterial = Mat(new Color(1f, 0.8f, 0.1f), new Color(1.5f, 1.1f, 0.1f));
        var pk = go.AddComponent<Pickup>(); pk.kind = Pickup.Kind.SuperArrow;
    }

    static Material Mat(Color c, Color emission)
    {
        var sh = Shader.Find("Universal Render Pipeline/Lit"); if (sh == null) sh = Shader.Find("Standard");
        var m = new Material(sh) { color = c };
        m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", emission);
        return m;
    }
}
