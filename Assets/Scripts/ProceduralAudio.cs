using UnityEngine;

// Throwaway placeholder sounds, synthesized in code so the audio hooks are testable before any
// real .wav files exist. Once a real clip with the matching name is dropped into Resources/Audio,
// GameAudio uses that instead and these are never generated for that name.
// Keyword-mapped: GameAudio asks For("boss_slam") and gets a boom, For("bow_shoot") a pluck, etc.
public static class ProceduralAudio
{
    const int SR = 44100;

    public static AudioClip For(string name)
    {
        name = name.ToLowerInvariant();
        if (name.Contains("bow") || (name.Contains("arrow") && !name.Contains("hit"))) return Pluck("p_" + name, 430f, 170f, 0.18f);
        if (name.Contains("clink") || name.Contains("armor")) return Pluck("p_" + name, 950f, 760f, 0.10f);
        if (name.Contains("weak") || name.Contains("gore") || (name.Contains("hit") && name.Contains("arrow"))) return Thud("p_" + name, 0.13f, 1500f);
        if (name.Contains("monkey") || name.Contains("screech") || name.Contains("chatter")) return Screech("p_" + name, 0.45f);
        if (name.Contains("zombie") || name.Contains("monster") || name.Contains("die") || name.Contains("hurt") || name.Contains("roar")) return Growl("p_" + name, 0.42f);
        if (name.Contains("slam") || name.Contains("boom") || name.Contains("collapse") || name.Contains("bridge")) return Boom("p_" + name, 0.6f);
        if (name.Contains("windup") || name.Contains("whoosh") || name.Contains("sweep")) return Whoosh("p_" + name, 0.45f);
        if (name.Contains("barrel") || name.Contains("rumble")) return Rumble("p_" + name, 2.0f);
        if (name.Contains("tension") || name.Contains("strident") || name.Contains("alarm") || name.Contains("siren")) return Tension("p_" + name, 4.0f);
        if (name.Contains("ambient") || name.Contains("drone") || name.Contains("portal") || name.Contains("hum")) return Drone("p_" + name, 3.0f);
        if (name.Contains("fire")) return Fire("p_" + name, 2.0f);
        if (name.Contains("complete") || name.Contains("victory")) return Fanfare("p_" + name);
        if (name.Contains("smash") || name.Contains("wall") || name.Contains("crumble") || name.Contains("spike") || name.Contains("explos")) return Thud("p_" + name, 0.3f, 700f);
        // coins, ui, pickups, heart, super, chest, generic
        return Blip("p_" + name, 720f, 1080f, 0.12f);
    }

    // ---- helpers ----
    static AudioClip Make(string name, float[] d)
    {
        var clip = AudioClip.Create(name, d.Length, 1, SR, false);
        clip.SetData(d, 0);
        return clip;
    }

    // simple deterministic noise so a given name always sounds the same
    static float Noise(ref uint s) { s ^= s << 13; s ^= s >> 17; s ^= s << 5; return (s / 4294967295f) * 2f - 1f; }

    static AudioClip Pluck(string name, float f0, float f1, float dur)
    {
        int n = (int)(SR * dur); var d = new float[n]; float ph = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            ph += 2f * Mathf.PI * Mathf.Lerp(f0, f1, t) / SR;
            d[i] = Mathf.Sin(ph) * Mathf.Exp(-5f * t) * 0.55f;
        }
        return Make(name, d);
    }

    static AudioClip Blip(string name, float f0, float f1, float dur)
    {
        int n = (int)(SR * dur); var d = new float[n]; float ph = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            ph += 2f * Mathf.PI * Mathf.Lerp(f0, f1, t) / SR;
            float sq = Mathf.Sin(ph) > 0 ? 1f : -1f;
            d[i] = sq * Mathf.Exp(-7f * t) * 0.3f;
        }
        return Make(name, d);
    }

    static AudioClip Thud(string name, float dur, float cutoff)
    {
        int n = (int)(SR * dur); var d = new float[n]; uint s = 0x1234abcd; float lp = 0f;
        float a = Mathf.Clamp01(cutoff / SR);
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            lp += a * (Noise(ref s) - lp);                       // low-passed noise body (boosted — a low cutoff kills RMS)
            float click = Noise(ref s) * Mathf.Exp(-70f * t) * 0.6f; // sharp stab transient up front
            d[i] = Mathf.Clamp((lp * 4f + click) * Mathf.Exp(-9f * t), -1f, 1f) * 0.85f;
        }
        return Make(name, d);
    }

    static AudioClip Boom(string name, float dur)
    {
        int n = (int)(SR * dur); var d = new float[n]; uint s = 0x9e3779b9; float ph = 0f, lp = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            ph += 2f * Mathf.PI * Mathf.Lerp(90f, 45f, t) / SR;
            lp += 0.02f * (Noise(ref s) - lp);
            float body = Mathf.Sin(ph) * Mathf.Exp(-4f * t);
            float crack = lp * Mathf.Exp(-22f * t);
            d[i] = Mathf.Clamp(body * 0.7f + crack * 0.6f, -1f, 1f);
        }
        return Make(name, d);
    }

    static AudioClip Growl(string name, float dur)
    {
        int n = (int)(SR * dur); var d = new float[n]; float ph = 0f; uint s = 0xdeadbeef; float lp = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            float f = Mathf.Lerp(150f, 70f, t) * (1f + 0.05f * Mathf.Sin(t * 40f)); // wavering pitch
            ph += 2f * Mathf.PI * f / SR;
            lp += 0.08f * (Noise(ref s) - lp);
            float saw = (ph % (2f * Mathf.PI)) / Mathf.PI - 1f;
            float e = Mathf.Min(Mathf.Clamp01(t * 8f), Mathf.Clamp01((1f - t) * 3f));
            d[i] = (saw * 0.4f + lp * 0.5f) * e * 0.6f;
        }
        return Make(name, d);
    }

    // monkey screech: chaotic high warbling tone + a little noise grit
    static AudioClip Screech(string name, float dur)
    {
        int n = (int)(SR * dur); var d = new float[n]; float ph = 0f; uint s = 0xabcd1234; float lp = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            float f = 750f + 500f * Mathf.Sin(t * 55f) + 300f * Mathf.Sin(t * 13f); // erratic high warble
            ph += 2f * Mathf.PI * f / SR;
            lp += 0.3f * (Noise(ref s) - lp);
            float env = Mathf.Min(1f, t * 22f) * Mathf.Exp(-3.2f * t);
            d[i] = (Mathf.Sin(ph) * 0.6f + lp * 0.25f) * env * 0.5f;
        }
        return Make(name, d);
    }

    static AudioClip Whoosh(string name, float dur)
    {
        int n = (int)(SR * dur); var d = new float[n]; uint s = 0xa5a5f00d; float lp = 0f, lp2 = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            float a = Mathf.Lerp(0.02f, 0.25f, t);   // sweep filter open = whoosh
            lp += a * (Noise(ref s) - lp); lp2 += a * (lp - lp2);
            float e = Mathf.Sin(t * Mathf.PI);       // swell in and out
            d[i] = lp2 * e * 0.7f;
        }
        return Make(name, d);
    }

    static AudioClip Rumble(string name, float dur)
    {
        int n = (int)(SR * dur); var d = new float[n]; uint s = 0xc0ffee11; float lp = 0f, ph = 0f;
        for (int i = 0; i < n; i++)
        {
            lp += 0.015f * (Noise(ref s) - lp);
            ph += 2f * Mathf.PI * 55f / SR;
            d[i] = Mathf.Clamp((lp * 0.8f + Mathf.Sin(ph) * 0.2f) * 0.6f, -1f, 1f);
        }
        LoopFade(d);
        return Make(name, d);
    }

    // Strident boss-room dread: a dissonant high pair (tritone) detuned so it beats ~8Hz, over a
    // menacing sub. Shrill and unsettling — the instant the player crosses into the arena they FEEL it.
    static AudioClip Tension(string name, float dur)
    {
        int n = (int)(SR * dur); var d = new float[n];
        float p1 = 0f, p2 = 0f, p3 = 0f, plo = 0f, lp = 0f; uint s = 0x5151aaaa;
        for (int i = 0; i < n; i++)
        {
            p1  += 2f * Mathf.PI * 740f  / SR;
            p2  += 2f * Mathf.PI * 1046f / SR;   // ~tritone above 740 -> dissonant, shrill
            p3  += 2f * Mathf.PI * 748f  / SR;   // detuned vs p1 -> ~8Hz beating = unease
            plo += 2f * Mathf.PI * 57f   / SR;   // sub-bass dread under it
            lp  += 0.012f * (Noise(ref s) - lp); // faint air
            float shrill = Mathf.Sin(p1) * 0.45f + Mathf.Sin(p3) * 0.45f + Mathf.Sin(p2) * 0.4f;
            d[i] = Mathf.Clamp(shrill * 0.5f + Mathf.Sin(plo) * 0.35f + lp * 0.25f, -1f, 1f) * 0.5f;
        }
        LoopFade(d);
        return Make(name, d);
    }

    static AudioClip Drone(string name, float dur)
    {
        int n = (int)(SR * dur); var d = new float[n]; float p1 = 0f, p2 = 0f, p3 = 0f;
        for (int i = 0; i < n; i++)
        {
            p1 += 2f * Mathf.PI * 65f / SR;
            p2 += 2f * Mathf.PI * 98f / SR;
            p3 += 2f * Mathf.PI * 130.5f / SR;
            d[i] = (Mathf.Sin(p1) * 0.5f + Mathf.Sin(p2) * 0.25f + Mathf.Sin(p3) * 0.15f) * 0.25f;
        }
        LoopFade(d);
        return Make(name, d);
    }

    // Flame fwoosh: punchy filtered-noise burst with a fast attack + decay (one-shot on ignite,
    // not a quiet loop bed — so NO LoopFade, which would swallow the attack).
    static AudioClip Fire(string name, float dur)
    {
        int n = (int)(SR * dur); var d = new float[n]; uint s = 0xf1f1a0a0; float lp = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            lp += 0.3f * (Noise(ref s) - lp);                                  // airy body
            float crackle = Mathf.Abs(Noise(ref s)) > 0.96f ? Noise(ref s) : 0f; // spit/pops
            float env = Mathf.Min(1f, t * 35f) * Mathf.Exp(-3.2f * t);          // fast attack, exp decay
            d[i] = Mathf.Clamp((lp * 1.1f + crackle * 0.7f) * env, -1f, 1f) * 0.9f;
        }
        return Make(name, d);
    }

    static AudioClip Fanfare(string name)
    {
        float[] notes = { 392f, 523f, 659f, 784f }; // G C E G
        float dur = 0.9f; int n = (int)(SR * dur); var d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            int ni = Mathf.Clamp((int)(t * notes.Length), 0, notes.Length - 1);
            float local = (t * notes.Length) - ni;
            float ph = 2f * Mathf.PI * notes[ni] * (i / (float)SR);
            d[i] = Mathf.Sin(ph) * Mathf.Exp(-2.5f * local) * 0.4f;
        }
        return Make(name, d);
    }

    // crossfade ends so a looped clip doesn't click at the seam
    static void LoopFade(float[] d)
    {
        int f = Mathf.Min(2000, d.Length / 4);
        for (int i = 0; i < f; i++)
        {
            float k = (float)i / f;
            d[i] *= k;
            d[d.Length - 1 - i] *= k;
        }
    }
}
