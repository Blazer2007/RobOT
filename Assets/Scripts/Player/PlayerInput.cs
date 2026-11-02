// PlayerInput.cs
using UnityEngine;

public class PlayerInput : MonoBehaviour, IMoveInput
{
    [SerializeField] private float jumpBufferTime = 0.15f;

    private float lastJumpPressTime = float.NegativeInfinity;

    public Vector2 Move { get; private set; }

    private void Update()
    {
        Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetButtonDown("Jump"))
            lastJumpPressTime = Time.time;
    }

    public bool ConsumeJump()
    {
        if (Time.time - lastJumpPressTime > jumpBufferTime) return false;
        lastJumpPressTime = float.NegativeInfinity;
        return true;
    }
}