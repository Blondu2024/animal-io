using UnityEngine;

// A bridge of contiguous segments over a pit. Step on it and, after a short beat,
// the segments drop one-by-one from the start — chasing you. Sprint across before
// the collapse catches you (Indiana-Jones style). Resets every segment on respawn.
public class CollapseBridge : LevelHazard
{
    public Transform[] segments;     // ordered along +Z (entry first)
    public float startDelay = 0.4f;  // grace after first step
    public float segInterval = 0.16f;// time between each segment dropping

    Rigidbody[] rbs;
    Collider[] cols;
    Vector3[] home;
    Quaternion[] homeRot;
    bool armed, collapsing;
    float armTimer, t;
    int dropped;

    protected override void Start()
    {
        int n = segments != null ? segments.Length : 0;
        rbs = new Rigidbody[n]; cols = new Collider[n]; home = new Vector3[n]; homeRot = new Quaternion[n];
        for (int i = 0; i < n; i++)
        {
            var s = segments[i];
            home[i] = s.position; homeRot[i] = s.rotation;
            var rb = s.GetComponent<Rigidbody>(); if (rb == null) rb = s.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true; rb.useGravity = false; rbs[i] = rb;
            cols[i] = s.GetComponent<Collider>();
        }
        base.Start();
    }

    protected override void OnReset()
    {
        armed = false; collapsing = false; armTimer = 0f; t = 0f; dropped = 0;
        int n = segments != null ? segments.Length : 0;
        for (int i = 0; i < n; i++)
        {
            if (rbs[i] != null) { rbs[i].isKinematic = true; rbs[i].useGravity = false; }
            segments[i].position = home[i];
            segments[i].rotation = homeRot[i];
            if (cols[i] != null) cols[i].enabled = true;
        }
    }

    protected override void Tick()
    {
        if (!collapsing)
        {
            if (PlayerOnBridge())
            {
                if (!armed) { armed = true; armTimer = startDelay; }
                else { armTimer -= Time.deltaTime; if (armTimer <= 0f) { collapsing = true; t = 0f; GameAudio.Play("bridge_collapse", transform.position); } }
            }
            return;
        }

        t += Time.deltaTime;
        int shouldDrop = Mathf.FloorToInt(t / segInterval) + 1;
        while (dropped < shouldDrop && dropped < segments.Length)
        {
            if (rbs[dropped] != null) { rbs[dropped].isKinematic = false; rbs[dropped].useGravity = true; }
            if (cols[dropped] != null) cols[dropped].enabled = false;
            dropped++;
        }
    }

    bool PlayerOnBridge()
    {
        if (player == null || cols == null || cols.Length == 0 || cols[0] == null) return false;
        Bounds b = cols[0].bounds;
        Vector3 pp = player.position;
        return pp.x > b.min.x - 0.4f && pp.x < b.max.x + 0.4f
            && pp.z > b.min.z - 0.4f && pp.z < b.max.z + 0.4f
            && pp.y > b.max.y - 0.4f && pp.y < b.max.y + 2.2f;
    }
}
