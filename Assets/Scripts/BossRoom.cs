using System.Collections.Generic;
using UnityEngine;

// The level finale. Touching the entrance portal warps the player into a sealed boss chamber
// where a boss + zombies spawn. Clear them and the exit portal (to level 2) activates.
// Dying registers the boss room as a death-menu checkpoint so you re-fight instead of redoing the gauntlet.
public class BossRoom : LevelHazard
{
    public Portal entrancePortal;     // in the corridor, after the bridge
    public Portal exitPortal;         // hidden until the room is cleared
    public Transform chamberSpawn;    // where the player lands
    public Transform bossSpawn;
    public Transform[] zombieSpawns;
    public int bossHealth = 5;   // weak-point hits to kill
    public float chamberZ = 182f;     // player counts as "inside" past this Z
    public float bossScale = 2.2f;    // orangutan model scale (imposing)
    public float monkeyScale = 0.9f;  // hook-monkey model scale (smaller than boss)

    Boss boss;
    readonly List<Zombie> adds = new List<Zombie>();
    bool fighting, won, showNext, tensionOn;

    protected override void OnReset()
    {
        if (boss != null) Destroy(boss.gameObject); boss = null;
        foreach (var z in adds) if (z != null) Destroy(z.gameObject);
        adds.Clear();
        fighting = false; won = false; showNext = false;
        if (exitPortal != null) exitPortal.gameObject.SetActive(false);
        if (entrancePortal != null) entrancePortal.gameObject.SetActive(true);

        bool wasTension = tensionOn; tensionOn = false;

        // respawned INSIDE the chamber (chose "Camera Boss") -> re-start the fight
        if (player != null && player.position.z >= chamberZ) EnterFight();
        else if (wasTension && GameAudio.Instance != null) GameAudio.Instance.StartAmbient("ambient_dungeon");
    }

    public void EnterFight()
    {
        if (fighting) return;
        if (pe != null && chamberSpawn != null) pe.WarpTo(chamberSpawn.position);
        if (entrancePortal != null) entrancePortal.gameObject.SetActive(false);
        SpawnEnemies();
        fighting = true; won = false;
        if (GameAudio.Instance != null) GameAudio.Instance.StartAmbient("boss_tension"); // swap corridor ambient for strident dread
        tensionOn = true;
        if (DeathManager.Instance != null) DeathManager.Instance.UnlockBossRoom();
    }

    void SpawnEnemies()
    {
        // real orangutan model (Resources/Boss_Orangutan), falling back to a capsule if missing
        var bossPrefab = Resources.Load<GameObject>("Boss_Orangutan");
        var b = bossPrefab != null ? Instantiate(bossPrefab) : GameObject.CreatePrimitive(PrimitiveType.Capsule);
        b.name = "Boss"; b.transform.SetParent(transform);
        b.transform.position = bossSpawn != null ? bossSpawn.position : new Vector3(0f, 2.4f, 220f);
        b.transform.localScale = Vector3.one * bossScale;
        var bcol = b.GetComponent<CapsuleCollider>(); if (bcol == null) bcol = b.AddComponent<CapsuleCollider>();
        bcol.height = 1.6f; bcol.radius = 0.5f; bcol.center = new Vector3(0f, 0.8f, 0f);
        boss = b.GetComponent<Boss>(); if (boss == null) boss = b.AddComponent<Boss>();
        boss.maxHealth = bossHealth;

        adds.Clear();
        var monkeyPrefab = Resources.Load<GameObject>("Monkey");
        if (zombieSpawns != null)
            foreach (var sp in zombieSpawns)
            {
                if (sp == null) continue;
                var z = monkeyPrefab != null ? Instantiate(monkeyPrefab) : GameObject.CreatePrimitive(PrimitiveType.Capsule);
                z.name = "Zombie"; z.transform.SetParent(transform); z.transform.position = sp.position;
                z.transform.localScale = Vector3.one * monkeyScale;
                var zcol = z.GetComponent<CapsuleCollider>(); if (zcol == null) zcol = z.AddComponent<CapsuleCollider>();
                zcol.height = 1.6f; zcol.radius = 0.4f; zcol.center = new Vector3(0f, 0.8f, 0f);
                var zc = z.GetComponent<Zombie>(); if (zc == null) zc = z.AddComponent<Zombie>();
                zc.speed = 3f; zc.health = 1; zc.killReward = 10;
                adds.Add(zc);
            }
    }

    protected override void Tick()
    {
        if (!fighting || won) return;
        adds.RemoveAll(z => z == null);
        if (boss == null && adds.Count == 0) Win();
    }

    void Win()
    {
        won = true; fighting = false;
        if (GameAudio.Instance != null) GameAudio.Instance.StartAmbient("ambient_dungeon"); // calm returns after the kill
        tensionOn = false;
        if (exitPortal != null) exitPortal.gameObject.SetActive(true);
        if (CurrencyManager.Instance != null) CurrencyManager.Instance.Add(100);
    }

    public void GoNextLevel()
    {
        showNext = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void RestartLevel()
    {
        if (pe == null) return;
        pe.SetSpawn(new Vector3(0f, 1f, 0f));
        var seq = Object.FindFirstObjectByType<LevelSequencer>();
        if (seq != null) seq.pendingSkipDynamic = false;
        pe.DoRespawn();
    }

    void OnGUI()
    {
        if (fighting && boss != null)
        {
            float w = 440f, x = (Screen.width - w) * 0.5f, y = 26f;
            GUI.color = new Color(0f, 0f, 0f, 0.6f); GUI.DrawTexture(new Rect(x - 3, y - 3, w + 6, 28), Texture2D.whiteTexture);
            float frac = Mathf.Clamp01(boss.HealthFrac);
            GUI.color = new Color(0.85f, 0.12f, 0.12f); GUI.DrawTexture(new Rect(x, y, w * frac, 22), Texture2D.whiteTexture);
            GUI.color = Color.white;
            var s = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            s.normal.textColor = Color.white;
            string hint = boss.IsExposed ? "BOSS — LOVEȘTE MIEZUL GALBEN!" : "BOSS — blindat (ferește securea)";
            GUI.Label(new Rect(x, y, w, 22), hint, s);
        }

        if (showNext)
        {
            GUI.color = new Color(0f, 0f, 0f, 0.82f); GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.color = Color.white;
            var t = new GUIStyle(GUI.skin.label) { fontSize = 34, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            t.normal.textColor = new Color(0.3f, 1f, 0.5f);
            GUI.Label(new Rect(0, Screen.height / 2 - 90, Screen.width, 50), "NIVEL 1 TERMINAT!", t);
            var s = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter }; s.normal.textColor = Color.white;
            GUI.Label(new Rect(0, Screen.height / 2 - 30, Screen.width, 30), "Level 2 — în construcție", s);
            float bw = 240f, bx = (Screen.width - bw) * 0.5f;
            if (GUI.Button(new Rect(bx, Screen.height / 2 + 24, bw, 48), "Înapoi la Level 1"))
            {
                showNext = false; Time.timeScale = 1f; RestartLevel();
            }
        }
    }
}
