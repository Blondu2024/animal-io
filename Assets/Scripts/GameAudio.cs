using System.Collections.Generic;
using UnityEngine;

// Central game audio. Loads every AudioClip placed under Assets/Resources/Audio (keyed by lowercased
// file name) and plays them on game events. If a requested sound has no real file yet, it falls back
// to a procedural placeholder (see ProceduralAudio) — so the hooks work immediately and a real .wav
// dropped into Resources/Audio later (same name) overrides the placeholder automatically.
//
// Event name -> what to drop in Resources/Audio:
//   bow_shoot, arrow_hit, weakpoint_hit, armor_clink, zombie_die, player_hurt,
//   boss_windup, boss_slam, boss_die, barrel_roll(loop), wall_smash, chest_open,
//   level_complete, ambient_dungeon(loop), footstep
public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance;

    public float masterVolume = 1f;
    public float sfxVolume = 0.9f;
    public float ambientVolume = 0.3f;

    readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
    readonly HashSet<string> realKeys = new HashSet<string>();      // names backed by an actual file
    readonly Dictionary<string, AudioSource> loops = new Dictionary<string, AudioSource>();
    AudioSource ambientSrc;

    // footstep ticker
    Transform player; CharacterController playerCC; float stepTimer;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        LoadAll();
    }

    void LoadAll()
    {
        foreach (var c in Resources.LoadAll<AudioClip>("Audio"))
            if (c != null) { var k = c.name.ToLowerInvariant(); clips[k] = c; realKeys.Add(k); }

        // existing clip already shipped at Resources/bow_release.wav -> use it for the bow
        if (!realKeys.Contains("bow_shoot"))
        {
            var bow = Resources.Load<AudioClip>("bow_release");
            if (bow != null) { clips["bow_shoot"] = bow; realKeys.Add("bow_shoot"); }
        }
    }

    AudioClip Get(string name)
    {
        name = name.ToLowerInvariant();
        if (clips.TryGetValue(name, out var c) && c != null) return c;
        var p = ProceduralAudio.For(name);   // synth + cache so we only build it once
        clips[name] = p;
        return p;
    }

    public bool HasReal(string name) => realKeys.Contains(name.ToLowerInvariant());

    // ---------- one-shots ----------
    public static void Play(string name, Vector3 pos, float vol = 1f, float pitch = 1f)
    {
        if (Instance != null) Instance.PlayAt(name, vol, pitch);
    }
    public static void Play2D(string name, float vol = 1f, float pitch = 1f)
    {
        if (Instance != null) Instance.PlayAt(name, vol, pitch);
    }

    void PlayAt(string name, float vol, float pitch)
    {
        var clip = Get(name);
        if (clip == null) return;
        var go = new GameObject("sfx:" + name);
        var src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.volume = Mathf.Clamp01(vol * sfxVolume * masterVolume);
        src.pitch = pitch;
        src.spatialBlend = 0f;              // 2D — simple, phone-friendly
        src.Play();
        Destroy(go, clip.length / Mathf.Max(0.1f, pitch) + 0.1f);
    }

    // ---------- loops (barrel, fire, portal) ----------
    public static void StartLoop(string name, float vol = 1f) { if (Instance != null) Instance.LoopOn(name, vol); }
    public static void StopLoop(string name) { if (Instance != null) Instance.LoopOff(name); }

    void LoopOn(string name, float vol)
    {
        if (loops.TryGetValue(name, out var ex) && ex != null)
        {
            ex.volume = Mathf.Clamp01(vol * sfxVolume * masterVolume);
            if (!ex.isPlaying) ex.Play();
            return;
        }
        var clip = Get(name);
        if (clip == null) return;
        var go = new GameObject("loop:" + name); go.transform.SetParent(transform);
        var src = go.AddComponent<AudioSource>();
        src.clip = clip; src.loop = true; src.spatialBlend = 0f;
        src.volume = Mathf.Clamp01(vol * sfxVolume * masterVolume);
        src.Play();
        loops[name] = src;
    }

    void LoopOff(string name)
    {
        if (loops.TryGetValue(name, out var src) && src != null) src.Stop();
    }

    // ---------- ambient bed ----------
    public void StartAmbient(string name = "ambient_dungeon")
    {
        var clip = Get(name);
        if (clip == null) return;
        if (ambientSrc == null)
        {
            var go = new GameObject("ambient"); go.transform.SetParent(transform);
            ambientSrc = go.AddComponent<AudioSource>();
            ambientSrc.loop = true; ambientSrc.spatialBlend = 0f;
        }
        ambientSrc.clip = clip;
        ambientSrc.volume = Mathf.Clamp01(ambientVolume * masterVolume);
        ambientSrc.Play();
    }

    // ---------- footsteps (only if a real footstep clip exists — no synth taps, they'd annoy) ----------
    void Update()
    {
        if (!realKeys.Contains("footstep")) return;
        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) { player = p.transform; playerCC = p.GetComponent<CharacterController>(); }
            if (player == null) return;
        }
        if (playerCC == null) return;
        Vector3 v = playerCC.velocity; v.y = 0f;
        if (playerCC.isGrounded && v.magnitude > 1.5f)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                stepTimer = Mathf.Lerp(0.5f, 0.28f, Mathf.InverseLerp(1.5f, 9f, v.magnitude));
                PlayAt("footstep", 0.5f, Random.Range(0.92f, 1.08f));
            }
        }
        else stepTimer = 0f;
    }
}
