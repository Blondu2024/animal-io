using UnityEngine;
using UnityEngine.Rendering;

// Level 1 opening: dark corridor -> (sound) -> light snaps on -> barrel rolls in.
public class HookSequence : MonoBehaviour
{
    public Light mainLight;
    public Light revealLight;
    public RollingHazard barrel;
    public AudioSource warnSound; // optional (assign a clip later)

    public float darkTime = 1.6f;
    public float darkIntensity = 0.06f;
    public float brightIntensity = 1.1f;

    float t;
    bool revealed;

    void Start()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.04f, 0.04f, 0.06f);
        if (mainLight != null) mainLight.intensity = darkIntensity;
        if (revealLight != null) revealLight.enabled = false;
        if (warnSound != null) warnSound.Play(); // rumble cue if a clip is set
    }

    void Update()
    {
        if (revealed) return;
        t += Time.deltaTime;
        if (t >= darkTime)
        {
            revealed = true;
            if (mainLight != null) mainLight.intensity = brightIntensity;
            if (revealLight != null) revealLight.enabled = true;
            RenderSettings.ambientLight = new Color(0.45f, 0.45f, 0.5f);
            if (barrel != null) barrel.Launch();
        }
    }
}
