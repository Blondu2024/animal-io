using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// The level brain. Runs stages in order:
//   barrel -> zombies -> panel0 -> zombies -> panel1 -> zombies -> panel2 -> zombies -> DONE
// Each obstacle must be passed / each zombie wave cleared to advance.
// Dying restarts the whole sequence from the barrel.
public class LevelSequencer : MonoBehaviour
{
    public RollingHazard barrel;
    public MovingPanel[] panels;   // Gap, Jump, Slide
    public Light mainLight;
    public Light revealLight;

    public int zombiesPerWave = 3;
    public float interStageDelay = 1.2f;
    public int passReward = 5;       // credits for clearing an obstacle
    public int completeReward = 50;  // credits for finishing the level
    public float darkIntensity = 0.06f;
    public float brightIntensity = 1.1f;
    public float panelAhead = 28f;

    [System.NonSerialized] public bool pendingSkipDynamic; // set by DeathManager: restart straight into the gauntlet
    [System.NonSerialized] public bool heldForIntro;       // IntroCinematic holds gameplay until the show ends

    // True once the dynamic intro (barrel/combat/panels) is cleared and only the gauntlet remains.
    public bool IsDone => stage >= 1 + (panels != null ? panels.Length : 0) * 2;

    Transform player;
    PlatformerExtras pe;
    int stage;
    int lastRespawn;
    float delayTimer;
    bool stageStarted;
    bool revealedOnce;
    readonly List<Zombie> wave = new List<Zombie>();
    GameObject combatBarrier;
    GameObject combatChest;
    int combatWave;

    enum Kind { Barrel, Combat, Panel, Done }

    void Start()
    {
        var p = GameObject.FindWithTag("Player");
        if (p != null) { player = p.transform; pe = p.GetComponent<PlatformerExtras>(); }
        if (pe != null) lastRespawn = pe.respawnCount;
        if (p != null && p.GetComponent<PlayerHealth>() == null) p.AddComponent<PlayerHealth>();

        if (CurrencyManager.Instance == null)
        {
            var cm = new GameObject("Credits");
            cm.AddComponent<CurrencyManager>();
        }

        if (DeathManager.Instance == null)
        {
            var dm = new GameObject("DeathManager");
            dm.AddComponent<DeathManager>();
        }

        if (GameAudio.Instance == null)
        {
            var ga = new GameObject("GameAudio");
            ga.AddComponent<GameAudio>();
        }
        GameAudio.Instance.StartAmbient();

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.04f, 0.04f, 0.06f);
        if (mainLight != null) mainLight.intensity = darkIntensity;
        if (revealLight != null) revealLight.enabled = false;

        RestartSequence();
    }

    void Update()
    {
        if (player == null) return;
        if (heldForIntro) return; // wait for the intro cinematic to finish

        if (pe != null && pe.respawnCount != lastRespawn)
        {
            lastRespawn = pe.respawnCount;
            if (pendingSkipDynamic) { pendingSkipDynamic = false; SkipToDone(); }
            else RestartSequence();
            return;
        }

        if (delayTimer > 0f) { delayTimer -= Time.deltaTime; return; }

        switch (KindOf(stage))
        {
            case Kind.Barrel:
                if (!stageStarted)
                {
                    if (!revealedOnce) { Reveal(); revealedOnce = true; }
                    ClearCombatProps();
                    if (barrel != null) barrel.Launch();
                    stageStarted = true;
                }
                else if (barrel == null || barrel.passed) Advance();
                break;

            case Kind.Panel:
                var panel = panels[PanelIndex(stage)];
                if (!stageStarted)
                {
                    ClearCombatProps();
                    if (panel != null) panel.LaunchFrom(new Vector3(0f, 0f, player.position.z + panelAhead));
                    stageStarted = true;
                }
                else if (panel == null || panel.done) Advance();
                break;

            case Kind.Combat:
                if (!stageStarted) { SpawnWave(); stageStarted = true; }
                else { wave.RemoveAll(z => z == null); if (wave.Count == 0) Advance(); }
                break;

            case Kind.Done:
                break; // level complete (later: light portal -> next level)
        }
    }

    Kind KindOf(int s)
    {
        int lastStage = 1 + (panels != null ? panels.Length : 0) * 2; // barrel + (panel+combat)*N
        if (s == 0) return Kind.Barrel;
        if (s >= 1 + (panels != null ? panels.Length : 0) * 2) return Kind.Done;
        return (s % 2 == 1) ? Kind.Combat : Kind.Panel;
    }

    int PanelIndex(int s) { return (s / 2) - 1; }

    void Advance()
    {
        var cleared = KindOf(stage);
        if (cleared == Kind.Barrel || cleared == Kind.Panel) AddCredits(passReward);

        stage++; stageStarted = false; delayTimer = interStageDelay;

        if (KindOf(stage) == Kind.Done) { AddCredits(completeReward); GameAudio.Play2D("level_complete"); }
    }

    void AddCredits(int n)
    {
        if (CurrencyManager.Instance != null) CurrencyManager.Instance.Add(n);
    }

    void Reveal()
    {
        if (mainLight != null) mainLight.intensity = brightIntensity;
        if (revealLight != null) revealLight.enabled = true;
        RenderSettings.ambientLight = new Color(0.45f, 0.45f, 0.5f);
    }

    // Restarting from a gauntlet checkpoint: skip the barrel/combat/panel intro, light it up,
    // clear any dynamic props, and park the sequencer at Done so only the gauntlet matters.
    void SkipToDone()
    {
        Reveal(); revealedOnce = true;
        foreach (var z in Object.FindObjectsByType<Zombie>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (z != null) Destroy(z.gameObject);
        wave.Clear();
        combatWave = 0;
        ClearCombatProps();
        if (barrel != null) barrel.ResetToStart();
        if (panels != null) foreach (var pn in panels) if (pn != null) pn.ResetPanel();
        stage = 1 + (panels != null ? panels.Length : 0) * 2; // Done
        stageStarted = true;
        delayTimer = 0f;
    }

    void RestartSequence()
    {
        stage = 0; stageStarted = false; delayTimer = 0.9f; // a beat before the barrel comes again
        foreach (var z in Object.FindObjectsByType<Zombie>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (z != null) Destroy(z.gameObject);
        wave.Clear();
        combatWave = 0;
        ClearCombatProps();
        if (barrel != null) barrel.ResetToStart();
        if (panels != null) foreach (var pn in panels) if (pn != null) pn.ResetPanel();
    }

    void SpawnWave()
    {
        wave.Clear();
        combatWave++;
        int count = 5 + (combatWave - 1) * 2; // more zombies each wave
        float pz = player.position.z;

        for (int i = 0; i < count; i++)
        {
            var pos = new Vector3(((i % 4) - 1.5f) * 1.4f, 1f, pz + 6f + (i / 4) * 2.5f); // in FRONT, spread
            var zc = Zombie.SpawnMonkey(pos, transform);   // real hook-monkey model + screech
            zc.speed = 3.0f; // faster -> harder
            wave.Add(zc);
        }

        // barrier ahead contains the player during combat
        combatBarrier = SpawnCube("CombatBarrier", new Vector3(0f, 1.7f, pz + 16f), new Vector3(6f, 3.4f, 0.5f), new Color(0.5f, 0.15f, 0.15f));

        // chest: open with F before the next obstacle destroys it
        combatChest = SpawnCube("Chest", new Vector3(1.6f, 0.5f, pz + 3f), new Vector3(1.0f, 0.9f, 0.9f), new Color(0.85f, 0.65f, 0.2f));
        var it = combatChest.AddComponent<Interactable>();
        it.kind = Interactable.Kind.Chest; it.playerAnimTrigger = "OpenTrig"; it.prompt = "Open chest"; it.coinReward = 50; it.destroyOnUse = true; it.grantSuperArrow = true;
    }

    GameObject SpawnCube(string name, Vector3 pos, Vector3 scale, Color col)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name; go.transform.SetParent(transform); go.transform.position = pos; go.transform.localScale = scale;
        var sh = Shader.Find("Universal Render Pipeline/Lit"); if (sh == null) sh = Shader.Find("Standard");
        go.GetComponent<Renderer>().sharedMaterial = new Material(sh) { color = col };
        return go;
    }

    void ClearCombatProps()
    {
        if (combatBarrier != null) { Destroy(combatBarrier); combatBarrier = null; }
        if (combatChest != null) { Destroy(combatChest); combatChest = null; } // destroyed if it wasn't opened
    }
}
