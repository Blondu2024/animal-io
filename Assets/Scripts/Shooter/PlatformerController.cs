using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlatformerController : MonoBehaviour
{
    [Header("Move")]
    public float walkSpeed = 5.5f;
    public float sprintSpeed = 9.5f;
    public float crawlSpeed = 2f;
    public float airControl = 0.55f;

    [Header("Jump")]
    public float jumpForce = 9.5f;
    public float gravityMul = 2.1f;
    public float lowJumpMul = 1.6f;
    public float coyoteTime = 0.12f;
    public float jumpBuffer = 0.12f;

    [Header("Dash (Q)")]
    public float dashSpeed = 18f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 0.8f;

    [Header("Roll (Ctrl)")]
    public float rollSpeed = 11f;
    public float rollDuration = 1.0f;
    public float rollCooldown = 1.3f;

    [Header("Climb (E)")]
    public float climbSpeed = 3f;
    public float climbCheckDist = 0.7f;

    [Header("Ground Check")]
    public float groundCheckDist = 0.18f;

    Rigidbody rb;
    CapsuleCollider col;
    Animator anim;
    Vector3 moveInput;
    Vector3 dashDir;
    Vector3 rollDir;

    float coyoteCounter, jumpBufferCounter;
    float dashCdCounter, dashTimer;
    float rollCdCounter, rollTimer;
    float lastWPressTime = -10f;
    public float doubleTapWindow = 0.3f;
    bool grounded, sprinting, crawling, climbing, jumpedThisFrame;
    Vector3 spawnPoint;

    public bool IsDashing => dashTimer > 0f;
    public bool IsRolling => rollTimer > 0f;
    public bool IsCrawling => crawling;
    public bool IsClimbing => climbing;
    public bool IsGrounded => grounded;
    public float CurrentSpeedNorm => Mathf.Clamp01(new Vector2(rb.linearVelocity.x, rb.linearVelocity.z).magnitude / sprintSpeed);
    public float DashCooldownNormalized => Mathf.Clamp01(1f - dashCdCounter / dashCooldown);
    public float RollCooldownNormalized => Mathf.Clamp01(1f - rollCdCounter / rollCooldown);

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.applyRootMotion = false;
        spawnPoint = transform.position;
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float h = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float v = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
        moveInput = new Vector3(h, 0f, v);
        if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();

        sprinting = (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed) && !crawling && !IsRolling;
        crawling = (kb.cKey.isPressed) && grounded && !IsRolling;

        if (kb.spaceKey.wasPressedThisFrame && !crawling) jumpBufferCounter = jumpBuffer;
        else jumpBufferCounter -= Time.deltaTime;

        if (kb.qKey.wasPressedThisFrame && dashCdCounter <= 0f && !IsRolling) StartDash();

        if (kb.wKey.wasPressedThisFrame)
        {
            if (Time.time - lastWPressTime < doubleTapWindow && rollCdCounter <= 0f && grounded && !crawling)
                StartRoll();
            lastWPressTime = Time.time;
        }

        bool eDown = kb.eKey.isPressed;
        if (climbing) climbing = eDown && WallInFront();
        else climbing = eDown && grounded && WallInFront();

        if (kb.backspaceKey.wasPressedThisFrame) Respawn();

        UpdateAnim();
    }

    void FixedUpdate()
    {
        CheckGround();
        ApplyMove();
        ApplyJump();
        ApplyExtraGravity();
        ApplyClimb();
        TickTimers();
    }

    void CheckGround()
    {
        float halfH = col.height * 0.5f * transform.lossyScale.y;
        Vector3 origin = transform.position + transform.TransformVector(col.center);
        grounded = Physics.Raycast(origin, Vector3.down, halfH + groundCheckDist,
                                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        if (grounded) coyoteCounter = coyoteTime;
        else coyoteCounter -= Time.fixedDeltaTime;
    }

    bool WallInFront()
    {
        return Physics.Raycast(transform.position + Vector3.up * 1f, transform.forward, climbCheckDist,
                               Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }

    void ApplyMove()
    {
        if (IsDashing)
        {
            rb.linearVelocity = new Vector3(dashDir.x * dashSpeed, rb.linearVelocity.y, dashDir.z * dashSpeed);
            return;
        }
        if (IsRolling)
        {
            Vector3 fwd = transform.forward;
            Vector3 side = transform.right * moveInput.x;
            Vector3 dir = (fwd + side * 0.6f).normalized;
            rb.linearVelocity = new Vector3(dir.x * rollSpeed, rb.linearVelocity.y, dir.z * rollSpeed);
            return;
        }
        if (climbing)
        {
            rb.linearVelocity = new Vector3(0f, climbSpeed, 0f);
            return;
        }
        if (crawling)
        {
            CrawlMove();
            return;
        }

        float speed = sprinting ? sprintSpeed : walkSpeed;
        Vector3 world = transform.TransformDirection(moveInput) * speed;
        float control = grounded ? 1f : airControl;
        Vector3 v = rb.linearVelocity;
        v.x = Mathf.Lerp(v.x, world.x, control * 0.35f);
        v.z = Mathf.Lerp(v.z, world.z, control * 0.35f);
        rb.linearVelocity = v;
    }

    void CrawlMove()
    {
        Vector3 v = rb.linearVelocity;
        if (moveInput.sqrMagnitude > 0.01f && Camera.main != null)
        {
            Vector3 fwd = Camera.main.transform.forward; fwd.y = 0f; fwd.Normalize();
            Vector3 right = Camera.main.transform.right;   right.y = 0f; right.Normalize();
            Vector3 worldDir = (fwd * moveInput.z + right * moveInput.x).normalized;
            Quaternion want = Quaternion.LookRotation(worldDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, want, 360f * Time.fixedDeltaTime);
            v.x = Mathf.Lerp(v.x, worldDir.x * crawlSpeed, 0.4f);
            v.z = Mathf.Lerp(v.z, worldDir.z * crawlSpeed, 0.4f);
        }
        else
        {
            v.x = Mathf.Lerp(v.x, 0f, 0.4f);
            v.z = Mathf.Lerp(v.z, 0f, 0.4f);
        }
        rb.linearVelocity = v;
    }

    void ApplyJump()
    {
        jumpedThisFrame = false;
        if (crawling || IsRolling) return;
        if (jumpBufferCounter <= 0f) return;
        bool canJump = climbing || coyoteCounter > 0f;
        if (!canJump) return;
        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
        climbing = false;
        Vector3 v = rb.linearVelocity;
        v.y = jumpForce;
        rb.linearVelocity = v;
        jumpedThisFrame = true;
    }

    void ApplyExtraGravity()
    {
        if (climbing || IsRolling) return;
        var v = rb.linearVelocity;
        var kb = Keyboard.current;
        if (v.y < 0f)
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (gravityMul - 1f) * Time.fixedDeltaTime;
        else if (v.y > 0f && (kb == null || !kb.spaceKey.isPressed))
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMul - 1f) * Time.fixedDeltaTime;
    }

    void ApplyClimb()
    {
        if (climbing)
        {
            rb.useGravity = false;
        }
        else
        {
            rb.useGravity = true;
        }
    }

    void StartDash()
    {
        Vector3 world = transform.TransformDirection(moveInput);
        dashDir = world.sqrMagnitude > 0.01f ? world.normalized : transform.forward;
        dashTimer = dashDuration;
        dashCdCounter = dashCooldown;
    }

    void StartRoll()
    {
        Vector3 world = transform.TransformDirection(moveInput);
        rollDir = world.sqrMagnitude > 0.01f ? world.normalized : transform.forward;
        rollTimer = rollDuration;
        rollCdCounter = rollCooldown;
    }

    void TickTimers()
    {
        if (dashTimer > 0f) dashTimer -= Time.fixedDeltaTime;
        if (dashCdCounter > 0f) dashCdCounter -= Time.fixedDeltaTime;
        if (rollTimer > 0f) rollTimer -= Time.fixedDeltaTime;
        if (rollCdCounter > 0f) rollCdCounter -= Time.fixedDeltaTime;
    }

    void UpdateAnim()
    {
        if (anim == null) return;
        anim.SetFloat("Speed", new Vector2(rb.linearVelocity.x, rb.linearVelocity.z).magnitude);
        anim.SetBool("Grounded", grounded);
        anim.SetBool("Sprinting", sprinting && moveInput.sqrMagnitude > 0.01f);
        anim.SetBool("Crawling", crawling);
        anim.SetBool("Climbing", climbing);
        anim.SetBool("Rolling", IsRolling);
        if (jumpedThisFrame) anim.SetTrigger("Jump");
    }

    public void SetSpawn(Vector3 p) => spawnPoint = p;

    public void Respawn()
    {
        rb.linearVelocity = Vector3.zero;
        transform.position = spawnPoint;
        dashTimer = 0f;
        rollTimer = 0f;
        crawling = false;
        climbing = false;
    }
}
