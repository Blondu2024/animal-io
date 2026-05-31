using UnityEngine;

// A flame jet that cycles ON/OFF. Sprint through during the OFF window;
// touching it while ON kills you. Put on a full-width cube (becomes a trigger).
[RequireComponent(typeof(Collider))]
public class FireTrap : LevelHazard
{
    public float onTime = 1.0f;
    public float offTime = 1.7f;
    public float phase = 0f;        // stagger multiple jets into a rhythm
    public float flameHeight = 3.4f;

    float t;
    bool hot, wasHot;
    Material mat;

    protected override void Start()
    {
        var r = GetComponent<Renderer>(); if (r != null) mat = r.material;
        var c = GetComponent<Collider>(); if (c != null) c.isTrigger = true;
        base.Start();
    }

    protected override void OnReset() { t = phase; wasHot = false; }

    protected override void Tick()
    {
        t += Time.deltaTime;
        float cycle = onTime + offTime;
        hot = Mathf.Repeat(t, cycle) < onTime;
        if (hot && !wasHot) GameAudio.Play("fire", transform.position, 0.7f); // fwoosh on ignite
        wasHot = hot;

        // grow fast when hot, drop when cold; keep the base planted on the floor (top y=0)
        float target = hot ? flameHeight : 0.05f;
        Vector3 s = transform.localScale;
        s.y = Mathf.MoveTowards(s.y, target, 22f * Time.deltaTime);
        transform.localScale = s;
        Vector3 p = transform.position; p.y = s.y * 0.5f; transform.position = p;

        if (mat != null)
        {
            mat.color = hot ? new Color(1f, 0.42f, 0.08f) : new Color(0.28f, 0.14f, 0.06f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", hot ? new Color(1.5f, 0.5f, 0.12f) : Color.black);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (hot && other.CompareTag("Player")) Kill();
    }
}
