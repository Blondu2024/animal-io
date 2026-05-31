using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(ThirdPersonController))]
[RequireComponent(typeof(StarterAssetsInputs))]
public class PlatformerExtras : MonoBehaviour
{
    [Header("Crawl (C hold) — crouch")]
    public float CrawlSpeed = 1.5f;
    public float CrawlScaleY = 0.5f;
    public float CrawlCameraY = 0.6f;
    public float CrawlOriginalCameraY = 1.375f;
    public float CrawlCCHeight = 1.0f;
    public float CrawlCCCenterY = 0.5f;
    public float CrawlTransitionSpeed = 5f;

    [Header("Roll Visual")]
    public float RollPivotHeight = 0.9f;

    [Header("Dash (Q tap)")]
    public float DashSpeed = 14f;
    public float DashDuration = 0.25f;
    public float DashCooldown = 0.8f;
    public float DashAccelRate = 80f;

    [Header("Roll (W double-tap)")]
    public float RollSpeed = 10f;
    public float RollDuration = 0.7f;
    public float RollCooldown = 1.5f;
    public float DoubleTapWindow = 0.3f;

    [Header("Climb (E hold + face wall)")]
    public float ClimbSpeed = 3f;
    public float ClimbCheckDist = 0.8f;

    [Header("Debug")]
    public bool DebugHUD = true;

    CharacterController cc;
    ThirdPersonController tpc;
    StarterAssetsInputs sai;
    Animator anim;

    bool crawling, dashing, rolling, climbing;
    float dashCdCounter, dashTimer;
    float rollCdCounter, rollTimer;
    float lastWPress = -10f;
    Vector3 dashStartPos, rollStartPos;

    float origMoveSpeed, origSprintSpeed, origSpeedChangeRate, origRotationSmoothTime;
    float origCCHeight, origCCCenterY;
    Vector3 spawnPoint;
    Transform skeletonRoot;
    Transform cameraRootTr;
    float crawlBlend;

    public bool IsDashing => dashing;
    public bool IsRolling => rolling;
    public bool IsCrawling => crawling;
    public bool IsClimbing => climbing;
    public float DashCooldownNormalized => Mathf.Clamp01(1f - dashCdCounter / Mathf.Max(0.001f, DashCooldown));
    public float RollCooldownNormalized => Mathf.Clamp01(1f - rollCdCounter / Mathf.Max(0.001f, RollCooldown));

    [System.NonSerialized] public int respawnCount; // bumps each death -> level restarts

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        tpc = GetComponent<ThirdPersonController>();
        sai = GetComponent<StarterAssetsInputs>();
        anim = GetComponentInChildren<Animator>();
        origMoveSpeed = tpc.MoveSpeed;
        origSprintSpeed = tpc.SprintSpeed;
        origSpeedChangeRate = tpc.SpeedChangeRate;
        origRotationSmoothTime = tpc.RotationSmoothTime;
        spawnPoint = transform.position;
        skeletonRoot = transform.Find("Skeleton");
        cameraRootTr = transform.Find("PlayerCameraRoot");
        if (cameraRootTr != null) CrawlOriginalCameraY = cameraRootTr.localPosition.y;
        origCCHeight = cc.height;
        origCCCenterY = cc.center.y;
    }

    void Update()
    {
        if (DeathManager.IsPaused) return;   // frozen while the death menu is open

        var kb = Keyboard.current;
        if (kb == null) return;

        sai.sprint = kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;

        if (kb.backspaceKey.wasPressedThisFrame) Respawn();

        UpdateDash(kb);
        UpdateRoll(kb);
        UpdateClimb(kb);
        UpdateCrawl(kb);

        if (anim != null)
        {
            anim.SetBool("Crawl", crawling);
            anim.SetBool("Climb", climbing);
            // Crawl animates only when actually moving; frozen pose when standing still
            float moveMag = sai.move.magnitude;
            anim.SetFloat("CrawlSpeedMult", crawling ? (moveMag > 0.1f ? 1f : 0f) : 1f);
        }

        if (dashing || rolling)
        {
            // Force the slide straight down the corridor (world +Z), regardless of camera angle
            float camYaw = Camera.main != null ? Camera.main.transform.eulerAngles.y : 0f;
            float r = -camYaw * Mathf.Deg2Rad;
            sai.move = new Vector2(Mathf.Sin(r), Mathf.Cos(r));
            sai.sprint = true;
        }

        TickTimers();
    }

    void LateUpdate()
    {
        if (climbing && cc.enabled)
        {
            cc.Move(Vector3.up * ClimbSpeed * Time.deltaTime);
        }
        if (skeletonRoot != null)
        {
            if (rolling)
            {
                float progress = 1f - Mathf.Clamp01(rollTimer / Mathf.Max(0.001f, RollDuration));
                float angleRad = progress * Mathf.PI * 2f;
                float liftY = RollPivotHeight * (1f - Mathf.Cos(angleRad));
                skeletonRoot.localPosition = new Vector3(0f, liftY, 0f);
                skeletonRoot.localRotation = Quaternion.AngleAxis(progress * 360f, Vector3.right);
            }
            else if (skeletonRoot.localRotation != Quaternion.identity || skeletonRoot.localPosition != Vector3.zero)
            {
                skeletonRoot.localRotation = Quaternion.identity;
                skeletonRoot.localPosition = Vector3.zero;
            }
        }
    }

    void UpdateCrawl(Keyboard kb)
    {
        bool wantCrawl = kb.cKey.isPressed && tpc.Grounded && !rolling && !dashing;
        if (wantCrawl != crawling)
        {
            crawling = wantCrawl;
            if (crawling)
            {
                tpc.MoveSpeed = CrawlSpeed;
                tpc.SprintSpeed = CrawlSpeed;
            }
            else if (!dashing && !rolling) RestoreSpeeds();
        }

        float target = crawling ? 1f : 0f;
        crawlBlend = Mathf.MoveTowards(crawlBlend, target, CrawlTransitionSpeed * Time.deltaTime);

        cc.height = Mathf.Lerp(origCCHeight, CrawlCCHeight, crawlBlend);
        Vector3 cen = cc.center; cen.y = Mathf.Lerp(origCCCenterY, CrawlCCCenterY, crawlBlend); cc.center = cen;

        // Visual squash removed — the real Mixamo crawl animation handles the low pose now.
        transform.localScale = Vector3.one;

        if (cameraRootTr != null)
        {
            Vector3 cp = cameraRootTr.localPosition;
            cp.y = Mathf.Lerp(CrawlOriginalCameraY, CrawlCameraY, crawlBlend);
            cameraRootTr.localPosition = cp;
        }
    }

    void UpdateDash(Keyboard kb)
    {
        if (!kb.qKey.wasPressedThisFrame || dashCdCounter > 0f || rolling || climbing) return;
        StartDash();
    }

    void UpdateRoll(Keyboard kb)
    {
        if (!kb.wKey.wasPressedThisFrame) return;
        bool ready = rollCdCounter <= 0f && !rolling && !crawling && !climbing && tpc.Grounded;
        if (Time.time - lastWPress < DoubleTapWindow && ready) StartRoll();
        lastWPress = Time.time;
    }

    void UpdateClimb(Keyboard kb)
    {
        bool wantClimb = kb.eKey.isPressed && WallInFront() && !dashing && !rolling && !crawling;
        if (climbing) climbing = wantClimb;
        else climbing = wantClimb && tpc.Grounded;
    }

    bool WallInFront()
    {
        return Physics.Raycast(transform.position + Vector3.up * 1f, transform.forward, ClimbCheckDist,
                               Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }

    void StartDash()
    {
        dashTimer = DashDuration;
        dashCdCounter = DashCooldown;
        dashing = true;
        dashStartPos = transform.position;
        tpc.MoveSpeed = DashSpeed;
        tpc.SprintSpeed = DashSpeed;
        tpc.SpeedChangeRate = DashAccelRate;
        tpc.RotationSmoothTime = 0.01f;
        // Temple-Run style slide: play feet-first slide animation + duck under obstacles
        if (anim != null) anim.SetTrigger("SlideTrig");
        cc.height = CrawlCCHeight;
        Vector3 dc = cc.center; dc.y = CrawlCCCenterY; cc.center = dc;
        Debug.Log($"[Extras] DASH/SLIDE START speed={DashSpeed} dur={DashDuration}");
    }

    void StartRoll()
    {
        rollTimer = RollDuration;
        rollCdCounter = RollCooldown;
        rolling = true;
        rollStartPos = transform.position;
        tpc.MoveSpeed = RollSpeed;
        tpc.SprintSpeed = RollSpeed;
        tpc.SpeedChangeRate = DashAccelRate;
        tpc.RotationSmoothTime = 0.01f;
        // Play the feet-first "Running Slide" animation as the roll + duck under
        if (anim != null) anim.SetTrigger("SlideTrig");
        cc.height = CrawlCCHeight;
        Vector3 rc = cc.center; rc.y = CrawlCCCenterY; cc.center = rc;
        Debug.Log($"[Extras] ROLL START speed={RollSpeed} dur={RollDuration}");
    }

    void RestoreSpeeds()
    {
        tpc.MoveSpeed = origMoveSpeed;
        tpc.SprintSpeed = origSprintSpeed;
        tpc.SpeedChangeRate = origSpeedChangeRate;
        tpc.RotationSmoothTime = origRotationSmoothTime;
    }

    void TickTimers()
    {
        if (dashTimer > 0f)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                dashing = false;
                if (!crawling) RestoreSpeeds();
                if (!crawling)
                {
                    cc.height = origCCHeight;
                    Vector3 dc = cc.center; dc.y = origCCCenterY; cc.center = dc;
                }
                Debug.Log($"[Extras] DASH END dist={(transform.position - dashStartPos).magnitude:0.00}m");
            }
        }
        if (dashCdCounter > 0f) dashCdCounter -= Time.deltaTime;
        if (rollTimer > 0f)
        {
            rollTimer -= Time.deltaTime;
            if (rollTimer <= 0f)
            {
                rolling = false;
                if (!crawling) RestoreSpeeds();
                if (!crawling)
                {
                    cc.height = origCCHeight;
                    Vector3 rc = cc.center; rc.y = origCCCenterY; cc.center = rc;
                }
                Debug.Log($"[Extras] ROLL END dist={(transform.position - rollStartPos).magnitude:0.00}m");
            }
        }
        if (rollCdCounter > 0f) rollCdCounter -= Time.deltaTime;
    }

    public void SetSpawn(Vector3 p) => spawnPoint = p;

    // Clean teleport that does NOT count as a death (no respawnCount bump). Used by the boss portal.
    public void WarpTo(Vector3 p)
    {
        cc.enabled = false;
        transform.position = p;
        cc.enabled = true;
    }

    // Death entry point. If a DeathManager exists, it intercepts to show the restart menu;
    // otherwise we just respawn immediately (fallback).
    public void Respawn()
    {
        if (DeathManager.Instance != null) { DeathManager.Instance.OnPlayerDied(); return; }
        DoRespawn();
    }

    // The actual teleport-to-spawn. Called directly by DeathManager after a checkpoint is chosen.
    public void DoRespawn()
    {
        respawnCount++;
        cc.enabled = false;
        transform.position = spawnPoint;
        cc.enabled = true;
        crawling = false;
        dashing = false;
        rolling = false;
        climbing = false;
        RestoreSpeeds();
        transform.localScale = Vector3.one;
        if (cameraRootTr != null) cameraRootTr.localPosition = new Vector3(0f, CrawlOriginalCameraY, 0f);
        if (skeletonRoot != null) { skeletonRoot.localRotation = Quaternion.identity; skeletonRoot.localPosition = Vector3.zero; }
    }

    void OnGUI()
    {
        if (!DebugHUD) return;
        var kb = Keyboard.current;
        var style = new GUIStyle(GUI.skin.label) { fontSize = 16 };
        style.normal.textColor = Color.white;
        GUI.color = new Color(0, 0, 0, 0.6f);
        GUI.Box(new Rect(10, Screen.height - 200, 540, 190), GUIContent.none);
        GUI.color = Color.white;
        float y = Screen.height - 195;
        GUI.Label(new Rect(20, y, 520, 20), $"Grounded(TPC)={tpc.Grounded}  Crawl={crawling}  Dash={dashing}  Roll={rolling}  Climb={climbing}", style); y += 22;
        if (kb != null)
        {
            GUI.Label(new Rect(20, y, 520, 20), $"KEYS: W={kb.wKey.isPressed} S={kb.sKey.isPressed} A={kb.aKey.isPressed} D={kb.dKey.isPressed} Space={kb.spaceKey.isPressed}", style); y += 22;
            GUI.Label(new Rect(20, y, 520, 20), $"      LShift={kb.leftShiftKey.isPressed}  C={kb.cKey.isPressed}  Q={kb.qKey.isPressed}  E={kb.eKey.isPressed}", style); y += 22;
        }
        GUI.Label(new Rect(20, y, 520, 20), $"TPC INPUT: move=({sai.move.x:0.0},{sai.move.y:0.0}) sprint={sai.sprint} jump={sai.jump}", style); y += 22;
        GUI.Label(new Rect(20, y, 520, 20), $"TPC: MoveSpeed={tpc.MoveSpeed:0.0}  SprintSpeed={tpc.SprintSpeed:0.0}  enabled={tpc.enabled}", style); y += 22;
        float t = Time.time - lastWPress;
        GUI.Label(new Rect(20, y, 520, 20), $"ROLL: lastW={t:0.00}s ago  cd={rollCdCounter:0.00}s  active={rolling}", style); y += 22;
        GUI.Label(new Rect(20, y, 520, 20), $"DASH cd={dashCdCounter:0.00}s  active={dashing}", style);
    }
}
