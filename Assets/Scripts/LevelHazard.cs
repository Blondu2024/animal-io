using UnityEngine;

// Base for STATIC traversal hazards in the corridor gauntlet (fire, spikes, crumble, bridge).
// Finds the player + PlatformerExtras once, then resets itself whenever the player respawns
// (same respawnCount-watch pattern the LevelSequencer uses). Fully self-contained per hazard,
// so each one is drop-in reusable on levels 2-10 without touching the sequencer.
public abstract class LevelHazard : MonoBehaviour
{
    protected Transform player;
    protected PlatformerExtras pe;
    int lastRespawn;
    bool ready;

    protected virtual void Start()
    {
        var p = GameObject.FindWithTag("Player");
        if (p != null) { player = p.transform; pe = p.GetComponent<PlatformerExtras>(); }
        if (pe != null) lastRespawn = pe.respawnCount;
        ready = true;
        OnReset();
    }

    protected virtual void Update()
    {
        if (!ready) return;
        if (pe != null && pe.respawnCount != lastRespawn) { lastRespawn = pe.respawnCount; OnReset(); }
        Tick();
    }

    // Kill the player (sends them back to the level start, bumps respawnCount -> everything resets).
    protected void Kill() { if (pe != null) pe.Respawn(); }

    protected virtual void OnReset() { }
    protected virtual void Tick() { }
}
