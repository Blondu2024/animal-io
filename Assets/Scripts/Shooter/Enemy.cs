using System.Collections.Generic;
using UnityEngine;

// Enemy with two behaviours:
//  - Melee: chases the hero and damages on contact.
//  - Ranged: keeps its distance and fires projectiles (punishes standing still in the aura).
// Registers itself so the aura can hit the nearest ones and the GameManager knows when a room is clear.
public class Enemy : MonoBehaviour
{
    public enum Mode { Melee, Ranged }

    public static readonly List<Enemy> All = new List<Enemy>();

    public float moveSpeed = 2.5f;
    public float maxHP = 30f;

    [Header("Melee")]
    public float touchDamage = 18f;
    public float touchInterval = 0.6f;

    [Header("Ranged")]
    public float rangedStopDistance = 6.5f;
    public float rangedFireInterval = 2.0f;
    public float rangedProjectileSpeed = 6.5f;
    public float rangedDamage = 14f;

    public Mode mode = Mode.Melee;

    [Header("Boss")]
    public bool isBoss;
    public float bossFireInterval = 2.2f;
    public int bossVolley = 8;
    float bossTimer;

    public static Enemy CurrentBoss;
    public float HPFraction => maxHP > 0f ? Mathf.Clamp01(hp / maxHP) : 0f;

    float hp;
    float touchTimer;
    float fireTimer;
    Transform player;
    HeroController hero;
    Renderer rend;
    Color baseColor = Color.white;
    float flashT;
    bool dying;

    RoomController room;
    Vector3 boundsCenter;
    float boundsHalf = 9.3f;
    bool hasBounds;

    public void SetRoom(RoomController r, Vector3 center, float half)
    {
        room = r;
        boundsCenter = center;
        boundsHalf = half;
        hasBounds = true;
    }

    public void MakeBoss()
    {
        isBoss = true;
        transform.localScale = Vector3.one * 2.2f;
        touchDamage = 26f;
        rangedProjectileSpeed = 7f;
        rangedDamage = 12f;
        bossTimer = 1.5f;
        CurrentBoss = this;
    }

    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    public void Init(float hpValue, float speed, Mode m)
    {
        maxHP = hpValue;
        hp = hpValue;
        moveSpeed = speed;
        mode = m;
    }

    void Start()
    {
        if (hp <= 0f) hp = maxHP;
        var p = GameObject.FindWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            hero = p.GetComponent<HeroController>();
        }
        // Stagger ranged volleys so they don't all fire on the same frame.
        fireTimer = rangedFireInterval * Random.Range(0.3f, 1f);

        rend = GetComponent<Renderer>();
        if (rend != null)
            baseColor = rend.material.HasProperty("_BaseColor")
                ? rend.material.GetColor("_BaseColor")
                : rend.material.color;
    }

    void Update()
    {
        if (flashT > 0f)
        {
            flashT -= Time.deltaTime;
            float k = Mathf.Clamp01(flashT / 0.12f);
            ShooterUtil.SetColor(gameObject, Color.Lerp(baseColor, Color.white, k));
        }

        if (player == null) return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;
        Vector3 toPlayerDir = dist > 0.001f ? toPlayer / dist : Vector3.zero;

        // Path around walls toward the player (through doorways).
        Vector3 wp = MapNav.NextWaypoint(transform.position, player.position);
        Vector3 moveDir = wp - transform.position;
        moveDir.y = 0f;
        if (moveDir.sqrMagnitude > 0.0001f) moveDir.Normalize();

        Vector3 face = moveDir;

        if (isBoss)
        {
            if (dist > 1.6f)
                transform.position += moveDir * moveSpeed * Time.deltaTime;
            else
                MeleeTouch();

            bossTimer -= Time.deltaTime;
            if (bossTimer <= 0f && !ShooterUtil.WallBetween(transform.position, player.position))
            {
                bossTimer = bossFireInterval;
                BossVolley(toPlayerDir);
            }
            face = toPlayerDir;
        }
        else if (mode == Mode.Melee)
        {
            if (dist > 1.2f)
                transform.position += moveDir * moveSpeed * Time.deltaTime;
            else
                { MeleeTouch(); face = toPlayerDir; }
        }
        else // Ranged: approach until it has a clear shot, then hold and fire.
        {
            bool canSee = !ShooterUtil.WallBetween(transform.position, player.position);
            if (canSee && dist <= rangedStopDistance + 0.5f)
            {
                if (dist < rangedStopDistance - 1.5f)
                    transform.position -= toPlayerDir * moveSpeed * Time.deltaTime;

                fireTimer -= Time.deltaTime;
                if (fireTimer <= 0f)
                {
                    fireTimer = rangedFireInterval;
                    FireProjectile(toPlayerDir);
                }
                face = toPlayerDir;
            }
            else
            {
                transform.position += moveDir * moveSpeed * Time.deltaTime;
            }
        }

        if (face.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(face);
    }

    void MeleeTouch()
    {
        touchTimer -= Time.deltaTime;
        if (touchTimer <= 0f)
        {
            touchTimer = touchInterval;
            if (hero != null) hero.TakeDamage(touchDamage);
        }
    }

    void BossVolley(Vector3 dir)
    {
        int n = Mathf.Max(1, bossVolley);
        float spread = 14f;
        float start = -spread * (n - 1) * 0.5f;
        for (int i = 0; i < n; i++)
        {
            Vector3 d = Quaternion.Euler(0f, start + spread * i, 0f) * dir;
            FireProjectile(d);
        }
    }

    void FireProjectile(Vector3 dir)
    {
        GameObject b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        b.name = "EnemyShot";
        b.transform.position = transform.position + dir * 0.7f + Vector3.up * 0.5f;
        b.transform.localScale = Vector3.one * 0.4f;

        var col = b.GetComponent<Collider>();
        col.isTrigger = true;

        var brb = b.AddComponent<Rigidbody>();
        brb.useGravity = false;
        brb.isKinematic = true;

        ShooterUtil.SetColor(b, new Color(1f, 0.4f, 0.9f));

        var proj = b.AddComponent<EnemyProjectile>();
        proj.Launch(dir, rangedProjectileSpeed, rangedDamage);
    }

    public void TakeDamage(float dmg) { TakeDamage(dmg, true); }

    public void TakeDamage(float dmg, bool fx)
    {
        if (dying) return;
        hp -= dmg;

        if (fx)
        {
            Sfx.Hit();
            flashT = 0.12f; // white hit flash (Update lerps it back)

            // Small knockback away from the player.
            if (player != null)
            {
                Vector3 kb = transform.position - player.position;
                kb.y = 0f;
                if (kb.sqrMagnitude > 0.0001f)
                    transform.position += kb.normalized * 0.22f;
            }
        }

        if (hp <= 0f)
        {
            dying = true;
            if (isBoss) CurrentBoss = null;
            Sfx.Death();
            PoofBit.Spawn(transform.position, baseColor);
            if (room != null) room.OnEnemyKilled();
            else if (GameManager.Instance != null) GameManager.Instance.OnEnemyKilled();
            Destroy(gameObject);
        }
    }

    // Closest living enemy to a world position.
    public static Enemy Nearest(Vector3 from)
    {
        Enemy best = null;
        float bestSqr = float.MaxValue;
        for (int i = 0; i < All.Count; i++)
        {
            Enemy e = All[i];
            if (e == null) continue;
            float d = (e.transform.position - from).sqrMagnitude;
            if (d < bestSqr) { bestSqr = d; best = e; }
        }
        return best;
    }
}
