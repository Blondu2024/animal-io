using UnityEngine;

// Procedural sound effects generated at runtime (no audio asset files needed).
// Played as 2D one-shots through a hidden AudioSource; the Main Camera has the AudioListener.
public static class Sfx
{
    static AudioSource src;
    static AudioClip shoot, hit, death, hurt, bowDraw, bowRelease;

    static void Ensure()
    {
        if (src != null) return;
        var go = new GameObject("Sfx");
        src = go.AddComponent<AudioSource>();
        src.spatialBlend = 0f;
        src.playOnAwake = false;

        shoot = Pluck(820f, 360f, 0.09f, 0.30f);
        hit   = Noise(0.07f, 0.35f, 0.35f);
        death = Noise(0.20f, 0.50f, 0.18f);
        hurt  = Pluck(200f, 110f, 0.18f, 0.50f);
        bowDraw    = Creak(0.34f, 0.26f);
        // Real recorded bow release (BigSoundBank, royalty-free) if present; else synth fallback.
        var realRelease = Resources.Load<AudioClip>("bow_release");
        bowRelease = realRelease != null ? realRelease : Thwip(0.18f, 0.55f);
    }

    public static void Shoot() { Ensure(); src.PlayOneShot(shoot); }
    public static void Hit()   { Ensure(); src.PlayOneShot(hit); }
    public static void Death() { Ensure(); src.PlayOneShot(death); }
    public static void Hurt()  { Ensure(); src.PlayOneShot(hurt); }
    public static void BowDraw()    { Ensure(); src.PlayOneShot(bowDraw); }
    public static void BowRelease() { Ensure(); src.PlayOneShot(bowRelease); }

    // A short tone that glides from f0 to f1 with an exponential decay (twang/pluck).
    static AudioClip Pluck(float f0, float f1, float dur, float vol)
    {
        int rate = 44100;
        int n = Mathf.Max(1, (int)(rate * dur));
        var data = new float[n];
        float phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            float f = Mathf.Lerp(f0, f1, t);
            phase += f / rate * 2f * Mathf.PI;
            float env = Mathf.Exp(-5f * t) * vol;
            data[i] = Mathf.Sin(phase) * env;
        }
        var clip = AudioClip.Create("sfx", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // Filtered noise burst (thud/impact). 'smooth' lowers the pitch/harshness.
    static AudioClip Noise(float dur, float vol, float smooth)
    {
        int rate = 44100;
        int n = Mathf.Max(1, (int)(rate * dur));
        var data = new float[n];
        float prev = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            float white = Random.Range(-1f, 1f);
            prev = Mathf.Lerp(prev, white, smooth);
            float env = Mathf.Exp(-6f * t) * vol;
            data[i] = prev * env;
        }
        var clip = AudioClip.Create("sfx", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // Irregular stepped (sample-and-hold) noise that rises — a wood/string creak (draw).
    static AudioClip Creak(float dur, float vol)
    {
        int rate = 44100;
        int n = Mathf.Max(1, (int)(rate * dur));
        var data = new float[n];
        float hold = 0f;
        int countdown = 0;
        int baseLen = Mathf.Max(1, rate / 90);
        for (int i = 0; i < n; i++)
        {
            if (countdown <= 0)
            {
                hold = Random.Range(-1f, 1f);
                countdown = Random.Range(baseLen / 2, baseLen * 2);
            }
            countdown--;
            float t = (float)i / n;
            float env = Mathf.Pow(t, 0.7f) * vol;
            data[i] = hold * env;
        }
        var clip = AudioClip.Create("sfx", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // Sharp snap + low thump + airy whoosh — a more convincing bow release.
    static AudioClip Thwip(float dur, float vol)
    {
        int rate = 44100;
        int n = Mathf.Max(1, (int)(rate * dur));
        var data = new float[n];
        float prev = 0f, phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            float white = Random.Range(-1f, 1f);
            prev = Mathf.Lerp(prev, white, 0.6f);
            float fl = Mathf.Lerp(160f, 70f, t);
            phase += fl / rate * 2f * Mathf.PI;
            float attack = Mathf.Exp(-30f * t);
            float body = Mathf.Exp(-12f * t);
            float whoosh = Mathf.Exp(-6f * t);
            float s = prev * 0.5f * attack + Mathf.Sin(phase) * 0.6f * body + prev * 0.25f * whoosh;
            data[i] = s * vol;
        }
        var clip = AudioClip.Create("sfx", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
