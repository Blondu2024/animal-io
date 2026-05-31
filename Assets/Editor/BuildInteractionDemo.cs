using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildInteractionDemo
{
    [MenuItem("MCP/Build Interaction Demo")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Demo] stop play first"); return; }
        var player = GameObject.Find("PlayerArmature");
        if (player == null) { Debug.LogError("[Demo] PlayerArmature not found"); return; }

        // Clean up any previous demo objects (idempotent)
        foreach (var n in new[] { "Door", "Chest", "Loot", "Rope", "Button", "GameSystems" })
        {
            var g = GameObject.Find(n);
            if (g != null) Object.DestroyImmediate(g);
        }
        foreach (var c in Object.FindObjectsByType<Coin>(FindObjectsSortMode.None))
            Object.DestroyImmediate(c.gameObject);

        var sys = new GameObject("GameSystems");
        sys.AddComponent<CurrencyManager>();
        if (player.GetComponent<PlayerInteractor>() == null) player.AddComponent<PlayerInteractor>();

        var door = Prim(PrimitiveType.Cube, "Door", new Vector3(0f, 1.5f, 6f), new Vector3(3f, 3f, 0.4f), new Color(0.35f, 0.3f, 0.45f));

        var chest = Prim(PrimitiveType.Cube, "Chest", new Vector3(2.5f, 0.5f, 2.5f), new Vector3(1.2f, 0.9f, 0.9f), new Color(0.8f, 0.6f, 0.2f));
        var ic = chest.AddComponent<Interactable>(); ic.kind = Interactable.Kind.Chest; ic.playerAnimTrigger = "OpenTrig"; ic.prompt = "Open chest"; ic.coinReward = 5;

        var loot = Prim(PrimitiveType.Cube, "Loot", new Vector3(2.5f, 0.4f, -2f), new Vector3(0.6f, 0.6f, 0.6f), new Color(0.2f, 0.7f, 0.9f));
        var il = loot.AddComponent<Interactable>(); il.kind = Interactable.Kind.Loot; il.playerAnimTrigger = "PickTrig"; il.prompt = "Pick up loot"; il.coinReward = 3;

        var rope = Prim(PrimitiveType.Cube, "Rope", new Vector3(-2.5f, 1.2f, 2.5f), new Vector3(0.15f, 2.4f, 0.15f), new Color(0.6f, 0.4f, 0.2f));
        var ir = rope.AddComponent<Interactable>();
        ir.kind = Interactable.Kind.Rope; ir.prompt = "Pull rope"; ir.doorToOpen = door.transform;
        ir.holdToOperate = true; ir.holdAnimBool = "Pulling"; ir.operateSpeed = 2f; ir.doorDropY = 3.2f;

        var btn = Prim(PrimitiveType.Cylinder, "Button", new Vector3(0f, 0.1f, 3.5f), new Vector3(0.9f, 0.1f, 0.9f), new Color(0.8f, 0.2f, 0.2f));
        var ib = btn.AddComponent<Interactable>(); ib.kind = Interactable.Kind.Button; ib.playerAnimTrigger = "ButtonTrig"; ib.prompt = "Push button"; ib.doorToOpen = door.transform;

        for (int i = 0; i < 5; i++)
        {
            var coin = Prim(PrimitiveType.Cylinder, "Coin" + i, new Vector3(-1.2f + i * 0.6f, 0.6f, -2.5f), new Vector3(0.4f, 0.05f, 0.4f), new Color(1f, 0.85f, 0.1f));
            coin.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            coin.GetComponent<Collider>().isTrigger = true;
            coin.AddComponent<Coin>();
        }

        EditorSceneManager.MarkSceneDirty(player.scene);
        EditorSceneManager.SaveScene(player.scene);
        Debug.Log("[Demo] DONE. Rope = hold-to-pull; others one-shot.");
    }

    static GameObject Prim(PrimitiveType t, string name, Vector3 pos, Vector3 scale, Color col)
    {
        var go = GameObject.CreatePrimitive(t);
        go.name = name; go.transform.position = pos; go.transform.localScale = scale;
        var sh = Shader.Find("Universal Render Pipeline/Lit"); if (sh == null) sh = Shader.Find("Standard");
        go.GetComponent<Renderer>().sharedMaterial = new Material(sh) { color = col };
        return go;
    }
}
