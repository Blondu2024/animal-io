using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Builds the Level-1 trap gauntlet AFTER the dynamic barrel/combat/panel section.
// Static traversal hazards (fire -> spikes -> crumble pit -> collapsing bridge), gray blockout.
// Idempotent: destroys any existing "Gauntlet" and rebuilds from scratch.
public static class BuildTrapGauntlet
{
    [MenuItem("MCP/Build Trap Gauntlet")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Gauntlet] stop play first"); return; }
        var corridor = GameObject.Find("Corridor");
        if (corridor == null) { Debug.LogError("[Gauntlet] no Corridor (run Build Hook / Build Level Sequence first)"); return; }

        var old = corridor.transform.Find("Gauntlet");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        var g = new GameObject("Gauntlet");
        g.transform.SetParent(corridor.transform);

        var matFloor = Mat(new Color(0.24f, 0.24f, 0.27f));
        var matWall  = Mat(new Color(0.17f, 0.17f, 0.21f));
        var matFire  = Mat(new Color(0.28f, 0.14f, 0.06f));
        var matSpike = Mat(new Color(0.55f, 0.55f, 0.6f));
        var matCrumb = Mat(new Color(0.4f, 0.35f, 0.28f));
        var matBridge= Mat(new Color(0.38f, 0.3f, 0.22f));
        var matVoid  = Mat(new Color(0.05f, 0.05f, 0.07f));

        float half = 3f;       // corridor half width (walls at +-3)
        float floorW = 6f;

        // ---- Floor: solid segments with PITS left open ----
        // S1 solid 72..114  (connector + fire + spikes)
        Floor(g, "Floor_S1", 93f, 42f, floorW, matFloor);
        // PIT (crumble)   114..128  -> open
        // S2 solid 128..132 (landing between pits)
        Floor(g, "Floor_S2", 130f, 4f, floorW, matFloor);
        // PIT (bridge)    132..150  -> open
        // S3 solid 150..164 (final landing, portal goes here later)
        Floor(g, "Floor_S3", 157f, 14f, floorW, matFloor);

        // ---- Walls: extend the corridor 72..164 ----
        Wall(g, "WallL_ext", -half, 118f, 92f, matWall);
        Wall(g, "WallR_ext",  half, 118f, 92f, matWall);

        // ---- KillPlane under both pits ----
        var kp = Prim(PrimitiveType.Cube, "KillPlane", new Vector3(0f, -5f, 132f), new Vector3(8f, 0.4f, 56f), matVoid, g.transform);
        kp.AddComponent<KillPlane>();

        // =====================  1) FIRE  =====================
        var fire = new GameObject("Fire"); fire.transform.SetParent(g.transform);
        float[] fz = { 86f, 90f, 94f };
        float[] fphase = { 0f, 0.9f, 1.8f };  // staggered rhythm
        for (int i = 0; i < fz.Length; i++)
        {
            var jet = Prim(PrimitiveType.Cube, "FireJet_" + i, new Vector3(0f, 0.05f, fz[i]), new Vector3(5.2f, 0.1f, 0.7f), matFire, fire.transform);
            var ft = jet.AddComponent<FireTrap>();
            ft.phase = fphase[i]; ft.onTime = 1.0f; ft.offTime = 1.7f; ft.flameHeight = 3.4f;
        }

        // =====================  2) SPIKES  =====================
        var spikes = new GameObject("Spikes"); spikes.transform.SetParent(g.transform);
        float[] sz = { 102f, 105f, 108f };
        for (int i = 0; i < sz.Length; i++)
        {
            var bed = Prim(PrimitiveType.Cube, "SpikeBed_" + i, new Vector3(0f, -1.4f, sz[i]), new Vector3(5.2f, 2.4f, 0.7f), matSpike, spikes.transform);
            var st = bed.AddComponent<SpikeTrap>();
            st.phase = 0f; st.upTime = 0.9f; st.downTime = 1.5f;  // synced -> clear "run when down" window
        }

        // =====================  3) CRUMBLE PIT  =====================
        var crumble = new GameObject("Crumble"); crumble.transform.SetParent(g.transform);
        float[] cz = { 115f, 118f, 121f, 124f, 127f };
        for (int i = 0; i < cz.Length; i++)
        {
            var plat = Prim(PrimitiveType.Cube, "Crumble_" + i, new Vector3(0f, -0.1f, cz[i]), new Vector3(2.4f, 0.4f, 2.4f), matCrumb, crumble.transform);
            plat.AddComponent<CrumblePlatform>();
        }

        // =====================  4) COLLAPSING BRIDGE  =====================
        var bridgeGO = new GameObject("Bridge"); bridgeGO.transform.SetParent(g.transform);
        int segCount = 9;
        var segs = new Transform[segCount];
        for (int i = 0; i < segCount; i++)
        {
            float z = 133f + i * 2f;  // 133..149 contiguous
            var seg = Prim(PrimitiveType.Cube, "BridgeSeg_" + i, new Vector3(0f, 0f, z), new Vector3(4f, 0.4f, 2f), matBridge, bridgeGO.transform);
            segs[i] = seg.transform;
        }
        var cb = bridgeGO.AddComponent<CollapseBridge>();
        cb.segments = segs;

        EditorSceneManager.MarkSceneDirty(corridor.scene);
        EditorSceneManager.SaveScene(corridor.scene);
        Debug.Log("[Gauntlet] DONE. fire(3) + spikes(3) + crumble(5) + bridge(9) built z86..149.");
    }

    static void Floor(GameObject parent, string name, float z, float len, float w, Material m)
    {
        Prim(PrimitiveType.Cube, name, new Vector3(0f, -0.1f, z), new Vector3(w, 0.2f, len), m, parent.transform);
    }

    static void Wall(GameObject parent, string name, float x, float z, float len, Material m)
    {
        Prim(PrimitiveType.Cube, name, new Vector3(x, 1.6f, z), new Vector3(0.3f, 3.4f, len), m, parent.transform);
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
