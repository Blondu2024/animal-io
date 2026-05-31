using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Full environment pass: recolor floor/walls to stone, ENCLOSE the corridor with a ceiling,
// line it densely with Synty columns, hang warm torches, and scatter props. Turns the gray
// box into a lit dungeon corridor. Decorative only — gameplay colliders untouched.
public static class BuildSyntyEnvironment
{
    const string Col = "Assets/Synty/PolygonStarter/Prefabs/SM_PolygonPrototype_Buildings_Column_2x3_01P.prefab";
    const string Crate = "Assets/Synty/PolygonStarter/Prefabs/SM_PolygonPrototype_Prop_Crate_03.prefab";
    const string Rock = "Assets/Synty/PolygonStarter/Prefabs/SM_Generic_Small_Rocks_01.prefab";

    static Material StoneWall, StoneFloor, StoneCol, Ceil, TorchMat, Wood;

    [MenuItem("MCP/Build Synty Environment")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Env] stop play first"); return; }
        var corridor = GameObject.Find("Corridor");
        if (corridor == null) { Debug.LogError("[Env] no Corridor"); return; }

        StoneWall  = Mat(new Color(0.27f, 0.24f, 0.21f));
        StoneFloor = Mat(new Color(0.36f, 0.32f, 0.28f));
        StoneCol   = Mat(new Color(0.42f, 0.39f, 0.35f));
        Ceil       = Mat(new Color(0.16f, 0.14f, 0.13f));
        TorchMat   = Mat(new Color(1f, 0.6f, 0.2f), new Color(2.2f, 1.0f, 0.3f));
        Wood       = Mat(new Color(0.4f, 0.26f, 0.13f));

        foreach (var n in new[] { "SyntyDress", "SyntyEnv" })
        {
            var o = corridor.transform.Find(n);
            if (o != null) Object.DestroyImmediate(o.gameObject);
        }
        var root = new GameObject("SyntyEnv"); root.transform.SetParent(corridor.transform);

        // 1) Recolor every floor/wall under Corridor + BossChamber to stone (by name; leaves traps/portals alone)
        RecolorWorld();

        // 2) Ceiling over the corridor (raised so the player doesn't poke through when jumping)
        //    + upper-wall strips to close the gap between the short gameplay walls and the new ceiling.
        var chamber = GameObject.Find("BossChamber");
        Box(root.transform, "Ceiling_Corridor", new Vector3(0f, 5.6f, 80.5f), new Vector3(6.4f, 0.3f, 167f), Ceil);
        Box(root.transform, "UpperWallL", new Vector3(-3f, 4.45f, 80.5f), new Vector3(0.3f, 2.5f, 167f), StoneWall);
        Box(root.transform, "UpperWallR", new Vector3( 3f, 4.45f, 80.5f), new Vector3(0.3f, 2.5f, 167f), StoneWall);
        if (chamber != null)
            Box(root.transform, "Ceiling_Chamber", new Vector3(0f, 5.2f, 205f), new Vector3(24.4f, 0.3f, 46f), Ceil);

        // SOLID back wall behind the start so the player can't walk off into the void
        SolidBox(root.transform, "BackWall_Start", new Vector3(0f, 2.8f, -3.3f), new Vector3(6.4f, 5.8f, 0.5f), StoneWall);

        var colPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Col);
        var cratePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Crate);
        var rockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Rock);

        // 3) Dense columns + torches along both corridor walls
        int torches = 0, cols = 0;
        for (float z = 3f; z <= 160f; z += 6f)
        {
            if (colPrefab != null)
            {
                PlaceCol(colPrefab, root.transform, new Vector3(-2.85f, 0f, z)); cols++;
                PlaceCol(colPrefab, root.transform, new Vector3( 2.85f, 0f, z)); cols++;
            }
        }
        for (float z = 8f; z <= 158f; z += 11f)
        {
            Torch(root.transform, new Vector3(-2.7f, 2.5f, z)); torches++;
            Torch(root.transform, new Vector3( 2.7f, 2.5f, z)); torches++;
        }

        // 4) Chamber columns
        if (colPrefab != null)
            for (float z = 186f; z <= 226f; z += 8f)
            {
                PlaceCol(colPrefab, root.transform, new Vector3(-11.4f, 0f, z));
                PlaceCol(colPrefab, root.transform, new Vector3( 11.4f, 0f, z));
            }

        // Chamber torches + central lights (the new ceiling blocks the overhead boss spot)
        if (chamber != null)
        {
            for (float z = 190f; z <= 224f; z += 8f)
            {
                Torch(root.transform, new Vector3(-11.2f, 2.7f, z));
                Torch(root.transform, new Vector3( 11.2f, 2.7f, z));
            }
            ChamberLight(root.transform, new Vector3(0f, 4.6f, 197f));
            ChamberLight(root.transform, new Vector3(0f, 4.6f, 216f));
        }

        // 5) Props scattered (crates recolored to wood so they sit on the warm stone)
        if (cratePrefab != null)
            foreach (var p in new[] { new Vector3(2.2f,0f,6f), new Vector3(-2.2f,0f,17f), new Vector3(2.1f,0f,40f), new Vector3(-9f,0f,190f), new Vector3(9f,0f,222f) })
            {
                var cr = PlaceProp(cratePrefab, root.transform, p, true); // solid crate
                foreach (var r in cr.GetComponentsInChildren<Renderer>()) r.sharedMaterial = Wood;
            }
        if (rockPrefab != null)
            foreach (var p in new[] { new Vector3(-2.4f,0f,10f), new Vector3(2.4f,0f,30f), new Vector3(-2.3f,0f,55f), new Vector3(2.3f,0f,140f) })
                PlaceProp(rockPrefab, root.transform, p, false); // rocks decorative

        EditorSceneManager.MarkSceneDirty(corridor.scene);
        EditorSceneManager.SaveScene(corridor.scene);
        Debug.Log($"[Env] DONE. Recolored stone + ceiling + {cols} columns + {torches} torches + props.");
    }

    static void RecolorWorld()
    {
        var roots = new List<Transform>();
        var c = GameObject.Find("Corridor"); if (c != null) roots.Add(c.transform);
        var bc = GameObject.Find("BossChamber"); if (bc != null) roots.Add(bc.transform);
        foreach (var rootT in roots)
            foreach (var r in rootT.GetComponentsInChildren<MeshRenderer>(true))
            {
                string nm = r.gameObject.name;
                if (nm.Contains("Floor")) r.sharedMaterial = StoneFloor;
                else if (nm.Contains("Wall")) r.sharedMaterial = StoneWall;
            }
    }

    static void Torch(Transform parent, Vector3 pos)
    {
        var bracket = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bracket.name = "Torch"; bracket.transform.SetParent(parent);
        bracket.transform.position = pos; bracket.transform.localScale = new Vector3(0.2f, 0.45f, 0.2f);
        Object.DestroyImmediate(bracket.GetComponent<Collider>());
        bracket.GetComponent<Renderer>().sharedMaterial = TorchMat;

        var lightGO = new GameObject("TorchLight"); lightGO.transform.SetParent(bracket.transform);
        lightGO.transform.localPosition = new Vector3(0f, 0.3f, pos.x > 0 ? -0.4f : 0.4f);
        var lt = lightGO.AddComponent<Light>();
        lt.type = LightType.Point; lt.color = new Color(1f, 0.62f, 0.28f); lt.range = 11f; lt.intensity = 3.2f;
    }

    static void PlaceCol(GameObject prefab, Transform parent, Vector3 pos)
    {
        var go = Instantiate(prefab, parent, pos, 0f, true); // solid
        foreach (var r in go.GetComponentsInChildren<Renderer>()) r.sharedMaterial = StoneCol;
    }

    static GameObject PlaceProp(GameObject prefab, Transform parent, Vector3 pos, bool solid)
    {
        return Instantiate(prefab, parent, pos, Random.Range(0f, 360f), solid);
    }

    static void ChamberLight(Transform parent, Vector3 pos)
    {
        var go = new GameObject("ChamberLight"); go.transform.SetParent(parent); go.transform.position = pos;
        var lt = go.AddComponent<Light>();
        lt.type = LightType.Point; lt.color = new Color(1f, 0.8f, 0.6f); lt.range = 26f; lt.intensity = 2.6f;
    }

    static GameObject Instantiate(GameObject prefab, Transform parent, Vector3 pos, float yRot, bool solid = false)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.transform.position = pos; go.transform.rotation = Quaternion.Euler(0f, yRot, 0f);
        var rends = go.GetComponentsInChildren<Renderer>();
        if (rends.Length > 0)
        {
            var b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            go.transform.position += new Vector3(0f, pos.y - b.min.y, 0f);
        }
        if (!solid)
            foreach (var col in go.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(col);
        return go;
    }

    static void Box(Transform parent, string name, Vector3 pos, Vector3 scale, Material m)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name; go.transform.SetParent(parent); go.transform.position = pos; go.transform.localScale = scale;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = m;
    }

    // Like Box but KEEPS its collider (player can't pass through it).
    static void SolidBox(Transform parent, string name, Vector3 pos, Vector3 scale, Material m)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name; go.transform.SetParent(parent); go.transform.position = pos; go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = m;
    }

    static Material Mat(Color c) { return Mat(c, Color.black); }
    static Material Mat(Color c, Color emission)
    {
        var sh = Shader.Find("Universal Render Pipeline/Lit"); if (sh == null) sh = Shader.Find("Standard");
        var m = new Material(sh) { color = c };
        if (emission != Color.black) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", emission); }
        return m;
    }
}
