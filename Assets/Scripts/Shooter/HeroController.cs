using UnityEngine;
using UnityEngine.InputSystem;

// Medieval archer. Move with WASD to dodge. While standing STILL and a target exists,
// you DRAW the bow (wind-up) then RELEASE an arrow — moving cancels the draw.
// Starts with a simple single shot; power/skills come from run upgrades (see GameManager).
[RequireComponent(typeof(Rigidbody))]
public class HeroController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float turnSpeed = 540f;   // deg/sec — smooth turning, not instant snap
    public float accel = 28f;        // units/sec^2 — ease in/out of movement

    [Header("Archery (auto-draw while standing still)")]
    public float fireInterval = 0.55f;  // draw time per arrow (lowered by attack-speed upgrades)
    public float bulletSpeed = 16f;
    public float bulletDamage = 12f;
    public int projectileCount = 1;
    public float spreadAngle = 12f;     // degrees between extra arrows

    [Header("Arrow skills (from upgrades)")]
    public int pierce = 0;
    public int bounce = 0;
    public float burnDps = 0f;
    public float burnDur = 0f;

    [Header("Health")]
    public float maxHP = 100f;
    [HideInInspector] public float hp;

    [Header("Defense")]
    public float damageReduction = 0f;  // 0..0.7, raised by armor/shield upgrades

    Rigidbody rb;
    Vector3 moveInput;
    bool drawing;
    float drawT;
    bool dead;
    bool justStopped = true;
    Animator anim;
    string animState;
    Vector3 vel;

    public bool IsDrawing => drawing;
    public float DrawNormalized => fireInterval <= 0f ? 0f : Mathf.Clamp01(drawT / fireInterval);

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        // Defensive defaults in case serialized values came in as 0.
        if (moveSpeed <= 0f) moveSpeed = 6f;
        if (fireInterval <= 0f) fireInterval = 0.55f;
        if (bulletSpeed <= 0f) bulletSpeed = 16f;
        if (bulletDamage <= 0f) bulletDamage = 12f;
        if (projectileCount <= 0) projectileCount = 1;
        if (maxHP <= 0f) maxHP = 100f;

        hp = maxHP;
        anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.applyRootMotion = false; // we drive position; animation is visual only
    }

    void SetAnim(string state)
    {
        if (anim == null || state == animState) return;
        animState = state;
        anim.CrossFade(state, 0.15f);
    }

    // Smoothly turn to face a horizontal direction (no instant snapping).
    void FaceDir(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        Quaternion goal = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, goal, turnSpeed * Time.deltaTime);
    }

    void Update()
    {
        if (dead) return;

        var kb = Keyboard.current;
        float h = 0f, v = 0f;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) h -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v -= 1f;
        }
        moveInput = new Vector3(h, 0f, v);
        if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();

        bool moving = moveInput.sqrMagnitude > 0.01f;
        if (moving)
        {
            SetAnim("Walk");
            // Face the way you walk (when standing & aiming, we face the target instead).
            FaceDir(moveInput);
            drawing = false; // moving cancels the draw
            drawT = 0f;
            justStopped = true;
            return;
        }

        Enemy target = NearestVisible();
        if (target == null)
        {
            SetAnim("Idle");
            drawing = false;
            drawT = 0f;
            return;
        }

        SetAnim("Attack"); // combat stance while aiming / shooting

        // Aim at the target throughout the draw.
        Vector3 aim = target.transform.position - transform.position;
        aim.y = 0f;
        FaceDir(aim);

        // Boost: the first arrow flies instantly when you stop, then normal draw cadence.
        if (justStopped)
        {
            justStopped = false;
            drawing = false;
            drawT = 0f;
            ReleaseArrow(aim.normalized);
            return;
        }

        if (!drawing)
        {
            drawing = true;
            drawT = 0f;
            Sfx.BowDraw();
        }

        drawT += Time.deltaTime;
        if (drawT >= fireInterval)
        {
            drawT = 0f;
            drawing = false; // a fresh draw begins next frame
            ReleaseArrow(aim.normalized);
        }
    }

    void FixedUpdate()
    {
        // Ease velocity toward the input so starts/stops feel weighty, not robotic.
        Vector3 desired = moveInput * moveSpeed;
        vel = Vector3.MoveTowards(vel, desired, accel * Time.fixedDeltaTime);
        // Walls (with doorways) physically contain the player across the room map.
        rb.MovePosition(rb.position + vel * Time.fixedDeltaTime);
    }

    void ReleaseArrow(Vector3 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();
        Sfx.BowRelease();

        // Fan out extra arrows symmetrically (multishot upgrades).
        int n = Mathf.Max(1, projectileCount);
        float start = -spreadAngle * (n - 1) * 0.5f;
        for (int i = 0; i < n; i++)
        {
            Vector3 d = Quaternion.Euler(0f, start + spreadAngle * i, 0f) * dir;
            SpawnArrow(d);
        }
    }

    // Nearest enemy with a clear line of sight (don't waste arrows on enemies behind walls).
    Enemy NearestVisible()
    {
        Enemy best = null;
        float bestSqr = float.MaxValue;
        var list = Enemy.All;
        for (int i = 0; i < list.Count; i++)
        {
            Enemy e = list[i];
            if (e == null) continue;
            if (ShooterUtil.WallBetween(transform.position, e.transform.position)) continue;
            float d = (e.transform.position - transform.position).sqrMagnitude;
            if (d < bestSqr) { bestSqr = d; best = e; }
        }
        return best;
    }

    void SpawnArrow(Vector3 dir)
    {
        // Arrow: a thin elongated box. Bullet orients it along its (arcing) velocity.
        GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
        b.name = "Arrow";
        b.transform.position = transform.position + dir * 0.6f + Vector3.up * 0.35f;
        b.transform.localScale = new Vector3(0.1f, 0.1f, 0.85f);

        var col = b.GetComponent<Collider>();
        col.isTrigger = true;

        var brb = b.AddComponent<Rigidbody>();
        brb.useGravity = false;
        brb.isKinematic = true;

        Color arrowColor = burnDps > 0f ? new Color(1f, 0.5f, 0.15f) : new Color(0.85f, 0.7f, 0.45f);
        ShooterUtil.SetColor(b, arrowColor);

        var bullet = b.AddComponent<Bullet>();
        bullet.Launch(dir, bulletSpeed, bulletDamage);
        bullet.Configure(pierce, bounce, burnDps, burnDur);
    }

    // ----- Upgrade hooks (called by GameManager when the player picks a reward) -----
    public void UpgAttackSpeed(float mult) { fireInterval = Mathf.Max(0.12f, fireInterval * mult); }
    public void UpgDamage(float mult) { bulletDamage *= mult; }
    public void UpgProjectile(int add) { projectileCount += add; }
    public void UpgMoveSpeed(float mult) { moveSpeed *= mult; }
    public void UpgBulletSpeed(float mult) { bulletSpeed *= mult; }
    public void UpgMaxHP(float add) { maxHP += add; hp = Mathf.Min(maxHP, hp + add); }
    public void UpgArmor(float add) { damageReduction = Mathf.Clamp(damageReduction + add, 0f, 0.7f); }
    public void UpgPierce(int n) { pierce += n; }
    public void UpgBounce(int n) { bounce += n; }
    public void UpgFire(float dps, float dur) { burnDps += dps; burnDur = Mathf.Max(burnDur, dur); }

    public void TakeDamage(float dmg)
    {
        if (dead) return;
        hp -= dmg * (1f - Mathf.Clamp01(damageReduction));
        Sfx.Hurt();
        if (hp <= 0f)
        {
            hp = 0f;
            dead = true;
            if (GameManager.Instance != null) GameManager.Instance.OnPlayerDeath();
        }
    }
}
