using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildHook
{
    [MenuItem("MCP/Build Hook (Level 1 opening)")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Hook] stop play first"); return; }
        var player = GameObject.Find("PlayerArmature");
        if (player == null) { Debug.LogError("[Hook] PlayerArmature not found"); return; }

        // Hide existing clutter so the corridor is clean (reversible — they're only deactivated)
        foreach (var n in new[] { "Map", "Demo_Level1", "Chest", "Loot", "Rope", "Button", "Door", "HookManager", "Corridor" })
        {
            var g = GameObject.Find(n);
            if (g != null)
            {
                if (n == "HookManager" || n == "Corridor") Object.DestroyImmediate(g); // rebuildable
                else g.SetActive(false);
            }
        }
        foreach (var c in Object.FindObjectsByType<Coin>(FindObjectsSortMode.None)) c.gameObject.SetActive(false);

        // Player at corridor start, facing +Z
        player.transform.position = new Vector3(0f, 1f, 0f);
        player.transform.rotation = Quaternion.identity;

        var corridor = new GameObject("Corridor");

        var matFloor = Mat(new Color(0.25f, 0.25f, 0.28f));
        var matWall = Mat(new Color(0.18f, 0.18f, 0.22f));

        var floor = Prim(PrimitiveType.Cube, "Floor", new Vector3(0f, -0.1f, 34.5f), new Vector3(6f, 0.2f, 75f), matFloor, corridor.transform);
        Prim(PrimitiveType.Cube, "WallL", new Vector3(-3f, 1.6f, 34.5f), new Vector3(0.3f, 3.4f, 75f), matWall, corridor.transform);
        Prim(PrimitiveType.Cube, "WallR", new Vector3(3f, 1.6f, 34.5f), new Vector3(0.3f, 3.4f, 75f), matWall, corridor.transform);

        // Reveal spotlight (off until the hook fires)
        var spotGO = new GameObject("RevealLight");
        spotGO.transform.SetParent(corridor.transform);
        spotGO.transform.position = new Vector3(0f, 6f, 6f);
        spotGO.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
        var spot = spotGO.AddComponent<Light>();
        spot.type = LightType.Spot; spot.range = 40f; spot.spotAngle = 90f; spot.intensity = 6f;
        spot.color = new Color(1f, 0.95f, 0.85f);

        // Barrel hazard (rolls toward player from far +Z)
        var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        barrel.name = "Barrel";
        barrel.transform.SetParent(corridor.transform);
        barrel.transform.position = new Vector3(0f, 0.9f, 28f);
        barrel.transform.rotation = Quaternion.Euler(0f, 0f, 90f); // axis across the corridor (X)
        barrel.transform.localScale = new Vector3(1.7f, 1.5f, 1.7f);
        barrel.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.5f, 0.32f, 0.15f));
        barrel.GetComponent<Collider>().isTrigger = true;
        var hazard = barrel.AddComponent<RollingHazard>();
        hazard.speed = 6f;

        // Hook orchestrator
        var dirLight = GameObject.Find("Directional Light");
        var mgr = new GameObject("HookManager");
        var seq = mgr.AddComponent<HookSequence>();
        if (dirLight != null) seq.mainLight = dirLight.GetComponent<Light>();
        seq.revealLight = spot;
        seq.barrel = hazard;

        EditorSceneManager.MarkSceneDirty(player.scene);
        EditorSceneManager.SaveScene(player.scene);
        Debug.Log("[Hook] DONE. Dark corridor + reveal + rolling barrel built.");
    }

    static Material Mat(Color c)
    {
        var sh = Shader.Find("Universal Render Pipeline/Lit"); if (sh == null) sh = Shader.Find("Standard");
        return new Material(sh) { color = c };
    }

    static GameObject Prim(PrimitiveType t, string name, Vector3 pos, Vector3 scale, Material mat, Transform parent)
    {
        var go = GameObject.CreatePrimitive(t);
        go.name = name; go.transform.SetParent(parent); go.transform.position = pos; go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }
}
