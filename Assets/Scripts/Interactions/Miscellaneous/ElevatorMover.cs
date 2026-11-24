using System;
using UnityEngine;

/// <summary>
/// Movimento do elevador (vai no GameObject que tem o Rigidbody, o mesmo da cabine).
/// Só se preocupa em mover: não conhece botões, portas nem animações.
/// - IMovingSurface: o Rob e o O.T são levados a bordo (como na MovingPlatform).
/// - IActivationSource: IsActive = "já partiu". Quem quiser reagir (portas da cabine a fechar,
///   por exemplo) usa um ActivationRequirement, sem este script saber que existe.
/// Estados: Idle -> Waiting (espera) -> Moving -> Arrived.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ElevatorMover : MonoBehaviour, IMovingSurface, IActivationSource, IResetlable
{
    private enum State { Idle, Waiting, Moving, Arrived }

    [SerializeField] private Vector3 moveOffset = new Vector3(0f, -3f, 0f); // deslocação em coordenadas do mundo
    [SerializeField][Range(1f, 3f)] private float speed = 3f;
    [SerializeField] private float startDelay = 2f; // espera entre carregar no botão e arrancar

    private Rigidbody body;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float timer;
    private State state = State.Idle;

    public Vector3 Velocity { get; private set; }
    public bool CanStart => state == State.Idle;
    public bool IsActive => state != State.Idle; // true = já partiu

    public event Action<bool> ActivationChanged;
    public event Action ElevatorArrived;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.interpolation = RigidbodyInterpolation.Interpolate;

        // Guardamos as posições uma só vez: o destino não "foge" a cada passo
        startPosition = transform.position;
        endPosition = startPosition + moveOffset;
    }

    public void StartDescent()
    {
        if (state != State.Idle) return;

        timer = startDelay;
        state = State.Waiting;
        ActivationChanged?.Invoke(true);
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case State.Waiting:
                Velocity = Vector3.zero;
                timer -= Time.fixedDeltaTime;
                if (timer <= 0f) state = State.Moving;
                break;

            case State.Moving:
                MoveStep();
                break;

            default:
                Velocity = Vector3.zero;
                break;
        }
    }

    private void MoveStep()
    {
        Vector3 current = body.position;
        Vector3 next = Vector3.MoveTowards(current, endPosition, speed * Time.fixedDeltaTime);

        Velocity = (next - current) / Time.fixedDeltaTime;
        body.MovePosition(next);

        if ((next - endPosition).sqrMagnitude < 0.000001f)
        {
            state = State.Arrived;
            Velocity = Vector3.zero;
            ElevatorArrived?.Invoke();
        }
    }

    public void ResetState()
    {
        bool wasActive = IsActive;

        state = State.Idle;
        timer = 0f;
        Velocity = Vector3.zero;

        transform.position = startPosition;
        body.position = startPosition;
        Physics.SyncTransforms();

        if (wasActive) ActivationChanged?.Invoke(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 from = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawLine(from, from + moveOffset);
        Gizmos.DrawWireSphere(from + moveOffset, 0.2f);
    }
}