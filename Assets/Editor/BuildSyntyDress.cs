using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// First Synty pass: dress the gray corridor with real Synty columns + crates so it reads as a
// designed level. Keeps all gameplay geometry/colliders untouched (purely decorative overlay).
// Idempotent: destroys any existing "SyntyDress" and rebuilds.
public static class BuildSyntyDress
{
    const string Col = "Assets/Synty/PolygonStarter/Prefabs/SM_PolygonPrototype_Buildings_Column_2x3_01P.prefab";
    const string Crate = "Assets/Synty/PolygonStarter/Prefabs/SM_PolygonPrototype_Prop_Crate_03.prefab";

    [MenuItem("MCP/Build Synty Dressing")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Synty] stop play first"); return; }
        var corridor = GameObject.Find("Corridor");
        if (corridor == null) { Debug.LogError("[Synty] no Corridor"); return; }

        var old = corridor.transform.Find("SyntyDress");
        if (old != null) Object.DestroyImmediate(old.gameObject);
        var root = new GameObject("SyntyDress");
        root.transform.SetParent(corridor.transform);

        var colPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Col);
        var cratePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Crate);
        if (colPrefab == null) { Debug.LogError("[Synty] column prefab not found: " + Col); return; }

        int n = 0;
        // Columns lining both corridor walls (x just inside the walls at +-3)
        for (float z = 4f; z <= 158f; z += 9f)
        {
            Place(colPrefab, root.transform, new Vector3(-2.85f, 0f, z), 0f); n++;
            Place(colPrefab, root.transform, new Vector3( 2.85f, 0f, z), 0f); n++;
        }
        // Columns around the boss chamber (x near the +-12 walls)
        for (float z = 188f; z <= 226f; z += 9.5f)
        {
            Place(colPrefab, root.transform, new Vector3(-11.4f, 0f, z), 0f); n++;
            Place(colPrefab, root.transform, new Vector3( 11.4f, 0f, z), 0f); n++;
        }

        // A few crates as set dressing near the opening
        if (cratePrefab != null)
        {
            foreach (var p in new[] { new Vector3(2.3f, 0f, 6f), new Vector3(-2.3f, 0f, 15f), new Vector3(2.2f, 0f, 23f) })
                Place(cratePrefab, root.transform, p, Random.Range(0f, 360f));
        }

        EditorSceneManager.MarkSceneDirty(corridor.scene);
        EditorSceneManager.SaveScene(corridor.scene);
        Debug.Log("[Synty] DONE. Placed " + n + " columns + crates. (decorative only, gameplay untouched)");
    }

    // Instantiate a Synty prefab, sit it on the floor (y=0), optional Y rotation.
    static void Place(GameObject prefab, Transform parent, Vector3 pos, float yRot)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0f, yRot, 0f);

        // drop it so its lowest point rests on the floor (y=0)
        var rends = go.GetComponentsInChildren<Renderer>();
        if (rends.Length > 0)
        {
            var b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            float lift = pos.y - b.min.y;
            go.transform.position += new Vector3(0f, lift, 0f);
        }
        // strip colliders so decoration never blocks the player
        foreach (var c in go.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
    }
}
