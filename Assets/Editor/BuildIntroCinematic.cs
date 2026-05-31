using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Sets up the opening cinematic: a barrel + a row of breakable walls across the corridor +
// hidden zombie groups behind each. The IntroCinematic component drives the show at play start.
// Idempotent: destroys existing "IntroCinematic" + "CinematicBarrel" and rebuilds.
public static class BuildIntroCinematic
{
    [MenuItem("MCP/Build Intro Cinematic")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Intro] stop play first"); return; }
        var corridor = GameObject.Find("Corridor");
        if (corridor == null) { Debug.LogError("[Intro] no Corridor"); return; }

        foreach (var n in new[] { "IntroCinematic", "CinematicBarrel" })
        {
            var o = GameObject.Find(n);
            if (o != null) Object.DestroyImmediate(o);
        }

        var stone = Mat(new Color(0.3f, 0.27f, 0.24f));
        var barrelMat = Mat(new Color(0.5f, 0.32f, 0.15f));
        var zombieMat = Mat(new Color(0.3f, 0.6f, 0.25f));

        // Cinematic barrel (visual only — it smashes walls by code)
        var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        barrel.name = "CinematicBarrel";
        barrel.transform.SetParent(corridor.transform);
        barrel.transform.position = new Vector3(0f, 0.9f, 2f);
        barrel.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        barrel.transform.localScale = new Vector3(1.7f, 1.5f, 1.7f);
        barrel.GetComponent<Renderer>().sharedMaterial = barrelMat;
        var bc = barrel.GetComponent<Collider>(); if (bc != null) Object.DestroyImmediate(bc);

        var walls = new List<BreakableWall>();
        var reveals = new List<GameObject>();
        float[] wz = { 18f, 36f, 54f, 72f };

        for (int w = 0; w < wz.Length; w++)
        {
            // breakable wall = grid of chunks filling the corridor cross-section
            var wallGO = new GameObject("BreakWall_" + w);
            wallGO.transform.SetParent(corridor.transform);
            wallGO.transform.position = new Vector3(0f, 0f, wz[w]);
            var chunks = new List<Transform>();
            float[] cx = { -1.7f, 0f, 1.7f };
            float[] cy = { 0.5f, 1.3f, 2.1f, 2.9f };
            foreach (var x in cx)
                foreach (var y in cy)
                {
                    var ch = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    ch.name = "Chunk"; ch.transform.SetParent(wallGO.transform);
                    ch.transform.position = new Vector3(x, y, wz[w]);
                    ch.transform.localScale = new Vector3(1.7f, 0.8f, 0.4f);
                    ch.GetComponent<Renderer>().sharedMaterial = stone;
                    chunks.Add(ch.transform);
                }
            var bw = wallGO.AddComponent<BreakableWall>();
            bw.chunks = chunks.ToArray();
            walls.Add(bw);

            // hidden zombie group revealed when this wall breaks
            var rev = new GameObject("Reveal_" + w);
            rev.transform.SetParent(corridor.transform);
            rev.transform.position = new Vector3(0f, 0f, wz[w] + 2.5f);
            foreach (var x in new[] { -1.1f, 1.1f })
            {
                var z = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                z.name = "Zombie"; z.transform.SetParent(rev.transform);
                z.transform.position = new Vector3(x, 1f, wz[w] + 2.5f);
                z.GetComponent<Renderer>().sharedMaterial = zombieMat;
                var zc = z.AddComponent<Zombie>(); zc.speed = 2.6f; zc.health = 2; zc.killReward = 10;
            }
            rev.SetActive(false);
            reveals.Add(rev);
        }

        var introGO = new GameObject("IntroCinematic");
        var intro = introGO.AddComponent<IntroCinematic>();
        intro.barrel = barrel.transform;
        intro.walls = walls.ToArray();
        intro.reveals = reveals.ToArray();
        var dir = GameObject.Find("Directional Light");
        if (dir != null) intro.mainLight = dir.GetComponent<Light>();
        intro.endZ = 90f;

        EditorSceneManager.MarkSceneDirty(corridor.scene);
        EditorSceneManager.SaveScene(corridor.scene);
        Debug.Log("[Intro] DONE. Barrel + " + walls.Count + " breakable walls + reveals. (Space/click skips the show.)");
    }

    static Material Mat(Color c)
    {
        var sh = Shader.Find("Universal Render Pipeline/Lit"); if (sh == null) sh = Shader.Find("Standard");
        return new Material(sh) { color = c };
    }
}
