using UnityEngine;
using StarterAssets;

public class PlayerInteractor : MonoBehaviour
{
    public float range = 2.4f;
    public KeyCode key = KeyCode.F;

    Animator anim;
    ThirdPersonController tpc;
    PlatformerExtras extras;
    Interactable current;
    Interactable[] interactables;

    // one-shot (chest / loot / button)
    bool busy;
    float busyTimer;
    Interactable pending;

    // hold (rope)
    Interactable holdTarget;

    void Start()
    {
        anim = GetComponent<Animator>();
        tpc = GetComponent<ThirdPersonController>();
        extras = GetComponent<PlatformerExtras>();
        interactables = Object.FindObjectsByType<Interactable>(FindObjectsSortMode.None);
    }

    void Update()
    {
        if (busy)
        {
            busyTimer -= Time.deltaTime;
            if (busyTimer <= 0f) EndOneShot();
            return;
        }

        if (holdTarget != null)
        {
            HandleHold();
            return;
        }

        current = FindNearest();
        if (current == null) return;

        if (current.holdToOperate)
        {
            if (Input.GetKey(key)) StartHold(current);
        }
        else if (Input.GetKeyDown(key))
        {
            BeginOneShot(current);
        }
    }

    Interactable FindNearest()
    {
        Interactable best = null;
        float bestDist = range;
        // find fresh each frame so runtime-spawned chests are detected
        foreach (var it in Object.FindObjectsByType<Interactable>(FindObjectsSortMode.None))
        {
            float d = Vector3.Distance(transform.position, it.transform.position);
            if (d < bestDist) { bestDist = d; best = it; }
        }
        return best;
    }

    // ---- one-shot ----
    void BeginOneShot(Interactable it)
    {
        busy = true; busyTimer = it.actionDuration; pending = it;
        if (anim != null && !string.IsNullOrEmpty(it.playerAnimTrigger)) anim.SetTrigger(it.playerAnimTrigger);
        Lock(true);
    }

    void EndOneShot()
    {
        if (pending != null) pending.Interact();
        pending = null; busy = false;
        Lock(false);
    }

    // ---- hold (only pulls while F is held, as long as held) ----
    void StartHold(Interactable it)
    {
        holdTarget = it;
        if (anim != null) anim.SetBool(it.holdAnimBool, true);
        Lock(true);
    }

    void HandleHold()
    {
        if (!Input.GetKey(key)) { StopHold(); return; }
        var it = holdTarget;
        if (it.doorToOpen != null && it.opened < it.doorDropY)
        {
            float step = Mathf.Min(it.operateSpeed * Time.deltaTime, it.doorDropY - it.opened);
            it.doorToOpen.position += Vector3.down * step;
            it.opened += step;
        }
    }

    void StopHold()
    {
        if (holdTarget != null && anim != null) anim.SetBool(holdTarget.holdAnimBool, false);
        holdTarget = null;
        Lock(false);
    }

    void Lock(bool on)
    {
        if (tpc != null) tpc.enabled = !on;
        if (extras != null) extras.enabled = !on;
        FollowCamera.Locked = on;
    }

    void OnGUI()
    {
        if (busy || holdTarget != null || current == null) return;
        var s = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        s.normal.textColor = Color.white;
        string verb = current.holdToOperate ? "Hold F: " : "[F] ";
        GUI.Label(new Rect(Screen.width / 2 - 160, Screen.height / 2 + 70, 320, 30), verb + current.prompt, s);
    }
}
