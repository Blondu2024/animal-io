using UnityEngine;

// Damage-over-time applied to an enemy by fire arrows. Ticks without the full hit FX.
public class Burn : MonoBehaviour
{
    Enemy enemy;
    float dps;
    float timeLeft;
    float tickTimer;

    void Awake() { enemy = GetComponent<Enemy>(); }

    public void Apply(float damagePerSec, float duration)
    {
        dps = Mathf.Max(dps, damagePerSec);
        timeLeft = Mathf.Max(timeLeft, duration);
    }

    void Update()
    {
        if (enemy == null) { Destroy(this); return; }

        timeLeft -= Time.deltaTime;
        tickTimer -= Time.deltaTime;
        if (tickTimer <= 0f)
        {
            tickTimer = 0.4f;
            enemy.TakeDamage(dps * 0.4f, false); // no flash/sound spam per tick
        }
        if (timeLeft <= 0f) Destroy(this);
    }
}
