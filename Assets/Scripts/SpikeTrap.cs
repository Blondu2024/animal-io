using UnityEngine;

// A spike bed that rises from the floor and retracts on a timer.
// Run across while it's DOWN, or jump it while UP. Touching it while raised kills you.
[RequireComponent(typeof(Collider))]
public class SpikeTrap : LevelHazard
{
    public float upTime = 0.9f;
    public float downTime = 1.5f;
    public float phase = 0f;
    public float riseY = 1.2f;     // world Y of the bed top when up
    public float sunkY = -1.4f;    // hidden below the floor when down

    float t;
    bool up, wasUp;

    protected override void Start()
    {
        var c = GetComponent<Collider>(); if (c != null) c.isTrigger = true;
        base.Start();
    }

    protected override void OnReset() { t = phase; wasUp = false; }

    protected override void Tick()
    {
        t += Time.deltaTime;
        float cycle = upTime + downTime;
        up = Mathf.Repeat(t, cycle) < upTime;
        if (up && !wasUp) GameAudio.Play("spike", transform.position, 0.8f); // chunky stab on rise
        wasUp = up;
        float target = up ? riseY : sunkY;
        Vector3 p = transform.position;
        p.y = Mathf.MoveTowards(p.y, target, 9f * Time.deltaTime);
        transform.position = p;
    }

    void OnTriggerStay(Collider other)
    {
        if (up && transform.position.y > riseY * 0.5f && other.CompareTag("Player")) Kill();
    }
}
