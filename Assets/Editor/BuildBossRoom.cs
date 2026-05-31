using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Builds the level finale: an entrance portal in the corridor (after the bridge) that warps the
// player into a sealed boss chamber, plus a hidden exit portal that activates once the room is cleared.
// Idempotent: destroys any existing "BossChamber" / "PortalA" and rebuilds.
public static class BuildBossRoom
{
    [MenuItem("MCP/Build Boss Room")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Boss] stop play first"); return; }
        var corridor = GameObject.Find("Corridor");
        if (corridor == null) { Debug.LogError("[Boss] no Corridor (build the level first)"); return; }

        foreach (var n in new[] { "BossChamber", "PortalA" })
        {
            var old = GameObject.Find(n);
            if (old != null) Object.DestroyImmediate(old);
        }

        var matFloor = Mat(new Color(0.22f, 0.2f, 0.26f), Color.black);
        var matWall = Mat(new Color(0.15f, 0.14f, 0.2f), Color.black);
        var matPortalIn = Mat(new Color(0.3f, 0.7f, 1f), new Color(0.3f, 0.7f, 1f));
        var matPortalOut = Mat(new Color(0.2f, 1f, 0.45f), new Color(0.2f, 1f, 0.45f));

        // ---- Chamber geometry (separate sealed room on +Z) ----
        var chamber = new GameObject("BossChamber");
        Prim(PrimitiveType.Cube, "BossFloor", new Vector3(0f, -0.1f, 205f), new Vector3(24f, 0.2f, 46f), matFloor, chamber.transform);
        Prim(PrimitiveType.Cube, "BossWallL", new Vector3(-12f, 2.4f, 205f), new Vector3(0.4f, 5f, 46f), matWall, chamber.transform);
        Prim(PrimitiveType.Cube, "BossWallR", new Vector3(12f, 2.4f, 205f), new Vector3(0.4f, 5f, 46f), matWall, chamber.transform);
        Prim(PrimitiveType.Cube, "BossWallBack", new Vector3(0f, 2.4f, 182f), new Vector3(24.4f, 5f, 0.5f), matWall, chamber.transform);
        Prim(PrimitiveType.Cube, "BossWallFront", new Vector3(0f, 2.4f, 228f), new Vector3(24.4f, 5f, 0.5f), matWall, chamber.transform);

        // ---- Arena light (so the chamber is always lit, independent of the corridor reveal) ----
        var lightGO = new GameObject("BossLight"); lightGO.transform.SetParent(chamber.transform);
        lightGO.transform.position = new Vector3(0f, 9f, 205f);
        lightGO.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        var lt = lightGO.AddComponent<Light>();
        lt.type = LightType.Spot; lt.range = 40f; lt.spotAngle = 120f; lt.intensity = 4f; lt.color = new Color(1f, 0.96f, 0.9f);

        // ---- Entrance portal A (in the corridor, spans the width so you can't dodge it) ----
        var portalA = Prim(PrimitiveType.Cube, "PortalA", new Vector3(0f, 1.7f, 160f), new Vector3(5.4f, 3.2f, 0.3f), matPortalIn, corridor.transform);
        var pa = portalA.AddComponent<Portal>(); pa.isExit = false;

        // ---- Exit portal B (in the chamber front, hidden until cleared) ----
        var portalB = Prim(PrimitiveType.Cube, "PortalB", new Vector3(0f, 1.7f, 226.5f), new Vector3(3f, 3.4f, 0.3f), matPortalOut, chamber.transform);
        var pb = portalB.AddComponent<Portal>(); pb.isExit = true;
        portalB.SetActive(false);

        // ---- Spawn points ----
        var chamberSpawn = Empty("ChamberSpawn", new Vector3(0f, 1f, 186f), chamber.transform);
        var bossSpawn = Empty("BossSpawn", new Vector3(0f, 2f, 220f), chamber.transform);
        var zSpawnRoot = new GameObject("ZombieSpawns"); zSpawnRoot.transform.SetParent(chamber.transform);
        var zPos = new[]
        {
            new Vector3(-6f, 1f, 210f), new Vector3(6f, 1f, 210f),
            new Vector3(-4f, 1f, 214f), new Vector3(4f, 1f, 214f), new Vector3(0f, 1f, 216f),
        };
        var zSpawns = new Transform[zPos.Length];
        for (int i = 0; i < zPos.Length; i++) zSpawns[i] = Empty("Z" + i, zPos[i], zSpawnRoot.transform).transform;

        // ---- Orchestrator ----
        var room = chamber.AddComponent<BossRoom>();
        room.entrancePortal = pa;
        room.exitPortal = pb;
        room.chamberSpawn = chamberSpawn.transform;
        room.bossSpawn = bossSpawn.transform;
        room.zombieSpawns = zSpawns;
        pa.room = room; pb.room = room;

        EditorSceneManager.MarkSceneDirty(corridor.scene);
        EditorSceneManager.SaveScene(corridor.scene);
        Debug.Log("[Boss] DONE. Entrance portal z160 -> chamber z182..228, boss + 5 zombies, exit portal hidden.");
    }

    static GameObject Empty(string name, Vector3 pos, Transform parent)
    {
        var go = new GameObject(name); go.transform.SetParent(parent); go.transform.position = pos; return go;
    }

    static Material Mat(Color c, Color emission)
    {
        var sh = Shader.Find("Universal Render Pipeline/Lit"); if (sh == null) sh = Shader.Find("Standard");
        var m = new Material(sh) { color = c };
        if (emission != Color.black) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", emission); }
        return m;
    }

    static GameObject Prim(PrimitiveType t, string name, Vector3 pos, Vector3 scale, Material mat, Transform parent)
    {
        var go = GameObject.CreatePrimitive(t);
        go.name = name; go.transform.SetParent(parent); go.transform.position = pos; go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }
}
