using UnityEditor;
using UnityEngine;

// Test-only helpers: drop the player straight onto a gauntlet section so the
// fall/collapse mechanics can be observed without playing through the whole level.
// Disables/re-enables the CharacterController so the teleport sticks (same trick as Respawn).
public static class TestTeleport
{
    [MenuItem("MCP/Test - Player To Crumble")]
    static void ToCrumble() => Put(new Vector3(0f, 1.4f, 118f));

    [MenuItem("MCP/Test - Player To Bridge")]
    static void ToBridge() => Put(new Vector3(0f, 1.4f, 133f));

    [MenuItem("MCP/Test - Player To Fire")]
    static void ToFire() => Put(new Vector3(0f, 1.2f, 84f));

    [MenuItem("MCP/Test - Player To Start")]
    static void ToStart() => Put(new Vector3(0f, 1f, 0f));

    [MenuItem("MCP/Test - Restart At Foc")]
    static void RestartFoc() { if (DeathManager.Instance != null) DeathManager.Instance.DebugRestart(1); }

    [MenuItem("MCP/Test - Restart At Start")]
    static void RestartStart() { if (DeathManager.Instance != null) DeathManager.Instance.DebugRestart(0); }

    [MenuItem("MCP/Test - Player To PortalA")]
    static void ToPortalA() => Put(new Vector3(0f, 1.4f, 160f));

    [MenuItem("MCP/Test - Enter Boss Room")]
    static void EnterBoss() { var r = Object.FindFirstObjectByType<BossRoom>(); if (r != null) r.EnterFight(); }

    static void Put(Vector3 p)
    {
        var go = GameObject.FindWithTag("Player");
        if (go == null) { Debug.LogError("[Test] no Player"); return; }
        var cc = go.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        go.transform.position = p;
        if (cc != null) cc.enabled = true;
        Debug.Log("[Test] player -> " + p.ToString("0.0"));
    }
}
