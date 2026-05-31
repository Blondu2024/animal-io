using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RollingHazard : MonoBehaviour
{
    public float speed = 6f;
    public float rollVisual = 200f;
    [HideInInspector] public bool passed;

    Vector3 startPos;
    Vector3 dir = Vector3.back;
    bool rolling;
    Transform player;

    void Awake()
    {
        startPos = transform.position;
    }

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        var p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    public void Launch()
    {
        transform.position = startPos;
        SetVisible(true);
        rolling = true;
        passed = false;
        GameAudio.StartLoop("barrel_roll", 1.5f);
    }

    public void ResetToStart()
    {
        transform.position = startPos;
        SetVisible(true);
        rolling = false;
        passed = false;
        GameAudio.StopLoop("barrel_roll");
    }

    void SetVisible(bool on)
    {
        var c = GetComponent<Collider>(); if (c != null) { c.enabled = on; c.isTrigger = true; }
        int childRenderers = 0;
        foreach (var rr in GetComponentsInChildren<Renderer>(true))
            if (rr.gameObject != gameObject) { rr.enabled = on; childRenderers++; }   // textured barrel prop
        var self = GetComponent<MeshRenderer>();
        if (self != null) self.enabled = (childRenderers == 0) && on;                 // grey cube only if no prop child
    }

    // Hide it without changing roll state — used to keep the dodge barrel out of sight
    // during the intro cinematic (the cinematic has its own separate barrel).
    public void Hide() { SetVisible(false); }

    void Update()
    {
        if (!rolling) return;
        transform.position += dir * speed * Time.deltaTime;
        transform.Rotate(Vector3.right * rollVisual * Time.deltaTime, Space.World);
        if (player != null && transform.position.z < player.position.z - 4f)
        {
            rolling = false; passed = true;
            SetVisible(false); // gone once it's behind you — no longer a death trap
            GameAudio.StopLoop("barrel_roll");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var ex = other.GetComponent<PlatformerExtras>();
        if (ex != null) ex.Respawn();
        rolling = false;
        GameAudio.StopLoop("barrel_roll");
    }
}
