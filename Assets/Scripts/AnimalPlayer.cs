using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Top-down player control for an animated animal.
/// WASD / arrows move it, it turns to face the direction it walks,
/// and switches between Idle and Walk animations based on movement.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class AnimalPlayer : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 14f;

    private Rigidbody _rb;
    private Animator _anim;
    private bool _isMoving;

    private void Awake()
    {
        Application.runInBackground = true; // keep ticking even when the Editor isn't focused
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        // Stay on the ground, never tip over, but free to turn around Y.
        _rb.constraints = RigidbodyConstraints.FreezePositionY
                        | RigidbodyConstraints.FreezeRotationX
                        | RigidbodyConstraints.FreezeRotationZ;

        _anim = GetComponentInChildren<Animator>();
        if (_anim != null) _anim.applyRootMotion = false; // movement is script-driven
    }

    private void FixedUpdate()
    {
        Keyboard kb = Keyboard.current;
        float x = 0f, z = 0f;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) z += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) z -= 1f;
        }

        Vector3 dir = new Vector3(x, 0f, z).normalized;
        Vector3 v = dir * moveSpeed;
        v.y = _rb.linearVelocity.y;
        _rb.linearVelocity = v;

        bool moving = dir.sqrMagnitude > 0.01f;
        if (moving)
        {
            Quaternion target = Quaternion.LookRotation(dir, Vector3.up);
            _rb.MoveRotation(Quaternion.Slerp(transform.rotation, target, turnSpeed * Time.fixedDeltaTime));
        }

        if (moving != _isMoving && _anim != null)
        {
            _isMoving = moving;
            _anim.CrossFadeInFixedTime(moving ? "Walk" : "Idle", 0.12f);
        }
    }
}
