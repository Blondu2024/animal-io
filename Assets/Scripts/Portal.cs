using UnityEngine;

// A glowing portal. Entrance portal (isExit=false) drops the player into the boss chamber;
// exit portal (isExit=true) triggers the "next level" screen. Pulses so it reads as interactive.
[RequireComponent(typeof(Collider))]
public class Portal : MonoBehaviour
{
    public BossRoom room;
    public bool isExit;

    Material mat;

    void Start()
    {
        var c = GetComponent<Collider>(); if (c != null) c.isTrigger = true;
        var r = GetComponent<Renderer>(); if (r != null) mat = r.material;
    }

    void Update()
    {
        if (mat == null) return;
        float p = 0.6f + 0.5f * Mathf.Sin(Time.unscaledTime * 3f);
        Color baseCol = isExit ? new Color(0.2f, 1f, 0.45f) : new Color(0.3f, 0.7f, 1f);
        mat.color = baseCol;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", baseCol * p);
    }

    void OnTriggerEnter(Collider other)
    {
        if (room == null || !other.CompareTag("Player")) return;
        if (isExit) room.GoNextLevel();
        else room.EnterFight();
    }
}
