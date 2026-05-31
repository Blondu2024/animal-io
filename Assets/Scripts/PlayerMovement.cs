using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple top-down movement for the player blob.
/// Uses the new Input System (WASD / arrow keys) read directly from the keyboard,
/// so it works without an Input Actions asset.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Tooltip("Movement speed in units per second.")]
    public float moveSpeed = 6f;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void FixedUpdate()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null)
        {
            return;
        }

        float x = 0f;
        float z = 0f;

        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) z += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) z -= 1f;

        Vector3 dir = new Vector3(x, 0f, z).normalized;
        _rb.linearVelocity = dir * moveSpeed;
    }
}
