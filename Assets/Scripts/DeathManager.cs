using UnityEngine;

// On death, pause the game and let the player choose where to restart instead of
// always going back to zero. Only checkpoints the player has actually reached are offered.
// Gauntlet checkpoints tell the LevelSequencer to skip the dynamic barrel/combat/panel intro.
public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance;
    public static bool IsPaused;

    [System.Serializable]
    public struct Checkpoint { public string label; public Vector3 pos; public bool skipDynamic; }

    public Checkpoint[] checkpoints =
    {
        new Checkpoint { label = "Start (butoi)", pos = new Vector3(0f, 1f, 0f),   skipDynamic = false },
        new Checkpoint { label = "Foc",          pos = new Vector3(0f, 1f, 80f),  skipDynamic = true  },
        new Checkpoint { label = "Tepi",         pos = new Vector3(0f, 1f, 98f),  skipDynamic = true  },
        new Checkpoint { label = "Crumble",      pos = new Vector3(0f, 1f, 112f), skipDynamic = true  },
        new Checkpoint { label = "Pod",          pos = new Vector3(0f, 1f, 129f), skipDynamic = true  },
        new Checkpoint { label = "Camera Boss",  pos = new Vector3(0f, 1f, 186f), skipDynamic = true  },
    };

    Transform player;
    PlatformerExtras pe;
    LevelSequencer seq;
    int maxReached;
    bool menuOpen;

    void Awake()
    {
        if (Instance == null) Instance = this; else { Destroy(this); return; }
        IsPaused = false;
    }

    void Start()
    {
        var p = GameObject.FindWithTag("Player");
        if (p != null) { player = p.transform; pe = p.GetComponent<PlatformerExtras>(); }
        seq = Object.FindFirstObjectByType<LevelSequencer>();
        maxReached = 0;
    }

    void Update()
    {
        if (menuOpen || player == null) return;
        // unlock checkpoints as the player passes their Z.
        // gauntlet checkpoints (skipDynamic) only unlock once the dynamic intro is actually cleared,
        // so you can't jump ahead to a section you never reached legitimately.
        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (i <= maxReached) continue;
            if (player.position.z < checkpoints[i].pos.z - 1f) continue;
            if (checkpoints[i].skipDynamic && (seq == null || !seq.IsDone)) continue;
            maxReached = i;
        }
    }

    // Called by PlatformerExtras.Respawn() instead of teleporting immediately.
    public void OnPlayerDied()
    {
        if (menuOpen) return;
        menuOpen = true;
        IsPaused = true;
        Time.timeScale = 0f;
        // free the cursor so the menu buttons are clickable (controllers lock it during play)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Boss room is entered via portal (not by walking past a Z), so it unlocks explicitly.
    public void UnlockBossRoom() { maxReached = checkpoints.Length - 1; }

    public void DebugRestart(int i) => Restart(i);   // test hook (MCP can't click IMGUI buttons)

    void Restart(int i)
    {
        var cp = checkpoints[Mathf.Clamp(i, 0, maxReached)];
        if (pe != null)
        {
            pe.SetSpawn(cp.pos);
            if (seq != null) seq.pendingSkipDynamic = cp.skipDynamic;
            pe.DoRespawn();
        }
        menuOpen = false;
        IsPaused = false;
        Time.timeScale = 1f;
    }

    void OnGUI()
    {
        if (!menuOpen) return;

        GUI.color = new Color(0f, 0f, 0f, 0.75f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float w = 380f, x = (Screen.width - w) * 0.5f;

        var title = new GUIStyle(GUI.skin.label) { fontSize = 30, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        title.normal.textColor = new Color(1f, 0.3f, 0.2f);
        GUI.Label(new Rect(x, 70, w, 50), "AI MURIT", title);

        var sub = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
        sub.normal.textColor = Color.white;
        GUI.Label(new Rect(x, 124, w, 28), "De unde reîncepi?", sub);
        int credits = CurrencyManager.Instance != null ? CurrencyManager.Instance.coins : 0;
        var cr = new GUIStyle(sub); cr.normal.textColor = new Color(1f, 0.85f, 0.1f);
        GUI.Label(new Rect(x, 150, w, 28), "◆ " + credits, cr);

        var btn = new GUIStyle(GUI.skin.button) { fontSize = 20, fontStyle = FontStyle.Bold };
        float y = 192f;
        for (int i = maxReached; i >= 0; i--) // furthest checkpoint first (most useful)
        {
            string label = (i == maxReached ? "▶ " : "   ") + checkpoints[i].label;
            if (GUI.Button(new Rect(x, y, w, 48), label, btn)) Restart(i);
            y += 56f;
        }
    }
}
