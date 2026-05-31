using UnityEngine;

// A floating, spinning collectible. Heart = +1 life; SuperArrow = temporary powered-up arrows.
[RequireComponent(typeof(Collider))]
public class Pickup : MonoBehaviour
{
    public enum Kind { Heart, SuperArrow }
    public Kind kind = Kind.Heart;
    public float superDuration = 8f;

    float baseY;

    void Start()
    {
        var c = GetComponent<Collider>(); if (c != null) c.isTrigger = true;
        baseY = transform.position.y;
    }

    void Update()
    {
        transform.Rotate(0f, 90f * Time.deltaTime, 0f);
        var p = transform.position;
        p.y = baseY + Mathf.Sin(Time.time * 2f) * 0.15f;
        transform.position = p;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (kind == Kind.Heart)
        {
            var hp = other.GetComponent<PlayerHealth>();
            if (hp != null) hp.Heal(1);
            GameAudio.Play("heart_pickup", transform.position);
        }
        else
        {
            var pc = other.GetComponent<PlayerCombat>();
            if (pc != null) pc.ActivateSuperArrow(superDuration);
            GameAudio.Play("superarrow_pickup", transform.position);
        }
        Destroy(gameObject);
    }
}
