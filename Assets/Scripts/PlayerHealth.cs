using UnityEngine;

// Player life — ONLY damaged by enemies (zombies/boss). Traps stay instant-death (handled by Respawn).
// Shown as red hearts top-left. Refills on respawn.
public class PlayerHealth : MonoBehaviour
{
    public int maxHearts = 6;
    public float invulnTime = 0.8f;   // brief i-frames so a zombie swarm can't drain you in one frame

    int hearts;
    float invuln;
    PlatformerExtras pe;
    int lastRespawn;

    public int Hearts => hearts;

    void Start()
    {
        pe = GetComponent<PlatformerExtras>();
        hearts = maxHearts;
        if (pe != null) lastRespawn = pe.respawnCount;
    }

    void Update()
    {
        if (invuln > 0f) invuln -= Time.deltaTime;
        if (pe != null && pe.respawnCount != lastRespawn) { lastRespawn = pe.respawnCount; hearts = maxHearts; invuln = 0f; }
    }

    // Enemy hit. At 0 hearts the player dies (-> death menu via Respawn).
    public void Damage(int n)
    {
        if (DeathManager.IsPaused || hearts <= 0 || invuln > 0f) return;
        hearts -= n;
        invuln = invulnTime;
        GameAudio.Play2D("player_hurt", 0.9f);
        if (hearts <= 0) { hearts = 0; if (pe != null) pe.Respawn(); }
    }

    public void Heal(int n) { hearts = Mathf.Clamp(hearts + n, 0, maxHearts); }

    void OnGUI()
    {
        var s = new GUIStyle(GUI.skin.label) { fontSize = 32, fontStyle = FontStyle.Bold };
        for (int i = 0; i < maxHearts; i++)
        {
            s.normal.textColor = i < hearts ? new Color(1f, 0.15f, 0.15f) : new Color(0.22f, 0.09f, 0.09f);
            GUI.Label(new Rect(14 + i * 36, 8, 40, 44), "♥", s);
        }
    }
}
