using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonCamera : MonoBehaviour
{
    public Transform body;
    public float sensitivity = 0.35f;
    public float minPitch = -45f;
    public float maxPitch = 70f;
    public float pivotHeight = 1.5f;
    public float distance = 2.8f;

    [Header("Auto-follow")]
    public float autoFollowDelay = 0.5f;
    public float autoFollowSpeed = 180f;
    public float mouseDeltaThreshold = 0.5f;
    public float velocityThreshold = 0.5f;

    Transform pivot;
    Rigidbody bodyRb;
    PlatformerController bodyPc;
    float pitch;
    float mouseIdleTimer;

    void Start()
    {
        if (body == null) body = transform.root;
        bodyRb = body.GetComponent<Rigidbody>();
        bodyPc = body.GetComponent<PlatformerController>();

        var go = new GameObject("CamPivot");
        pivot = go.transform;
        pivot.SetParent(body, false);
        pivot.localPosition = new Vector3(0f, pivotHeight, 0f);
        pivot.localRotation = Quaternion.identity;

        transform.SetParent(pivot, false);
        transform.localPosition = new Vector3(0f, 0f, -distance);
        transform.localRotation = Quaternion.identity;

        LockCursor(true);
    }

    void Update()
    {
        var mouse = Mouse.current;
        var kb = Keyboard.current;

        if (kb != null && kb.escapeKey.wasPressedThisFrame) LockCursor(false);
        if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked) LockCursor(true);

        if (mouse == null || Cursor.lockState != CursorLockMode.Locked) return;

        Vector2 d = mouse.delta.ReadValue();

        if (d.sqrMagnitude > mouseDeltaThreshold * mouseDeltaThreshold)
        {
            mouseIdleTimer = 0f;
            float yaw = d.x * sensitivity;
            pitch -= d.y * sensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            if (bodyPc == null || !bodyPc.IsCrawling) body.Rotate(0f, yaw, 0f, Space.World);
            pivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
        else
        {
            mouseIdleTimer += Time.deltaTime;
            if (mouseIdleTimer >= autoFollowDelay) AutoFollow();
        }
    }

    void AutoFollow()
    {
        if (bodyRb == null) return;
        Vector3 vel = new Vector3(bodyRb.linearVelocity.x, 0f, bodyRb.linearVelocity.z);
        if (vel.sqrMagnitude < velocityThreshold * velocityThreshold) return;
        Vector3 dir = vel.normalized;
        Vector3 fwd = body.forward; fwd.y = 0f; fwd.Normalize();
        if (Vector3.Dot(fwd, dir) < 0.3f) return;
        Quaternion want = Quaternion.LookRotation(dir);
        body.rotation = Quaternion.RotateTowards(body.rotation, want, autoFollowSpeed * Time.deltaTime);
    }

    void LockCursor(bool on)
    {
        Cursor.lockState = on ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !on;
    }
}
