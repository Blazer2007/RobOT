// PlayerMover.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PlayerMover : MonoBehaviour
{
    [SerializeField] private MovementData data; // SO
    [SerializeField] private MonoBehaviour cameraViewSource;
    [SerializeField] private MonoBehaviour inputSource; // IMoveInput a usar; vazio = o primeiro IMoveInput deste GameObject
    [SerializeField] private LayerMask groundLayers = ~0;

    private Rigidbody body;
    private Collider bodyCollider;
    private Collider lastGroundCollider;
    private IMovingSurface currentSurface; // 
    private Vector3 inheritedVelocity;   // velocidade herdada da última superfície em que pisou
    private IMoveInput input;
    private ICameraView cameraView;
    private IControllable character; // opcional; sem ele a personagem conta sempre como controlada

    public bool IsGrounded { get; private set; }

    // Quando não é o jogador a controlar, o input vem da IA (em coordenadas do mundo)
    // e a câmara não deve mexer nesta personagem
    private bool IsControlled => character == null || character.IsControlled;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        bodyCollider = GetComponent<Collider>();
        input = inputSource != null ? inputSource as IMoveInput : GetComponent<IMoveInput>();
        character = GetComponent<IControllable>();
        cameraView = cameraViewSource != null ? cameraViewSource.GetComponent<ICameraView>() : null;

        //if (data == null) Debug.LogError($"{name}: falta atribuir o MovementData.", this);
        //if (input == null) Debug.LogError($"{name}: não há PlayerInput (IMoveInput) neste GameObject.", this);
        //if (cameraView == null) Debug.LogError($"{name}: Camera View Source vazio ou sem ICameraView.", this);

        if (data == null || input == null || cameraView == null)
            enabled = false;
    }

    private void FixedUpdate()
    {
        CheckGround();
        Vector3 wishDir = GetWishDirection();
        ApplyHorizontalMovement(wishDir);
        TryJump();
        ApplyExtraGravity();
        Rotate(wishDir);
    }

    private void CheckGround()
    {
        Bounds b = bodyCollider.bounds;
        float radius = Mathf.Min(b.extents.x, b.extents.z) * 0.9f;
        float distance = b.extents.y - radius + data.GroundCheckDistance;

        bool hitGround = Physics.SphereCast(b.center, radius, Vector3.down, out RaycastHit hit,
            distance, groundLayers, QueryTriggerInteraction.Ignore);

        IsGrounded = hitGround && hit.normal.y >= Mathf.Cos(data.MaxSlopeAngle * Mathf.Deg2Rad);

        if (IsGrounded) UpdateSurface(hit.collider);
    }

    private void UpdateSurface(Collider ground)
    {
        if (ground != lastGroundCollider)
        {
            lastGroundCollider = ground;
            currentSurface = ground.GetComponentInParent<IMovingSurface>();
        }

        inheritedVelocity = currentSurface != null ? currentSurface.Velocity : Vector3.zero;
    }

    private Vector3 GetWishDirection()
    {
        Vector2 move = input.Move;
        Vector3 dir = Vector3.ClampMagnitude(new Vector3(move.x, 0f, move.y), 1f);
        return IsControlled ? cameraView.PlanarRotation * dir : dir;
    }

    private void ApplyHorizontalMovement(Vector3 wishDir)
    {
        Vector3 velocity = body.linearVelocity;
        Vector3 frame = new Vector3(inheritedVelocity.x, 0f, inheritedVelocity.z);

        // velocidade horizontal relativa à superfície onde está
        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z) - frame;
        Vector3 target = wishDir * data.MoveSpeed;

        float rate = wishDir.sqrMagnitude > 0.0001f ? data.Acceleration : data.Deceleration;
        if (!IsGrounded) rate *= data.AirControl;

        horizontal = Vector3.MoveTowards(horizontal, target, rate * Time.fixedDeltaTime);

        // plataforma a descer: acompanha-a em vez de "flutuar"
        float y = velocity.y;
        if (IsGrounded && inheritedVelocity.y < 0f) y = Mathf.Min(y, inheritedVelocity.y);

        body.linearVelocity = new Vector3(horizontal.x + frame.x, y, horizontal.z + frame.z);
    }

    private void TryJump()
    {
        if (!IsGrounded || !input.ConsumeJump()) return;

        float jumpSpeed = Mathf.Sqrt(2f * -Physics.gravity.y * data.JumpHeight);
        Vector3 velocity = body.linearVelocity;
        velocity.y = jumpSpeed;
        body.linearVelocity = velocity;
        IsGrounded = false;
    }

    private void ApplyExtraGravity()
    {
        if (body.linearVelocity.y < 0f)
            body.AddForce(Physics.gravity * (data.FallGravityMultiplier - 1f), ForceMode.Acceleration);
    }

    private void Rotate(Vector3 wishDir)
    {
        Quaternion target;
        if (IsControlled && cameraView.AlignsCharacterToView) target = cameraView.PlanarRotation;
        else if (wishDir.sqrMagnitude > 0.0001f) target = Quaternion.LookRotation(wishDir);
        else return;

        body.MoveRotation(Quaternion.RotateTowards(body.rotation, target, data.TurnSpeed * Time.fixedDeltaTime));
    }
}