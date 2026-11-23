using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour, IMovingSurface, IResetlable
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTime = 1f;
    [SerializeField] private MonoBehaviour activationSourceBehaviour; // opcional
    [Tooltip("Modo elevador: com a fonte ativa vai até ao último waypoint e fica lá; " +
             "sem ela volta ao primeiro. Desligado = vai e volta (ping-pong) enquanto a fonte estiver ativa.")]
    [SerializeField] private bool returnWhenInactive;

    private Rigidbody body;
    private int holdFrom; // modo elevador: waypoint de onde saiu
    private int holdTo;   // modo elevador: waypoint para onde vai
    private IActivationSource activationSource;
    private Vector3[] points;
    private int targetIndex = 1;
    private int direction = 1;
    private float waitTimer;
    private Vector3 startPosition;
    private Quaternion startRotation;

    public Vector3 Velocity { get; private set; }

    private bool IsRunning => activationSource == null || activationSource.IsActive;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.interpolation = RigidbodyInterpolation.Interpolate;

        if (activationSourceBehaviour != null)
            activationSource = activationSourceBehaviour.GetComponent<IActivationSource>();

        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.LogError($"{name}: são precisos pelo menos 2 waypoints.", this);
            enabled = false;
            return;
        }

        // Guardamos as posições uma só vez: se os waypoints forem filhos da plataforma, não a seguem.
        points = new Vector3[waypoints.Length];
        for (int i = 0; i < waypoints.Length; i++) points[i] = waypoints[i].position;

        startPosition = transform.position;
        startRotation = transform.rotation;
    }
    public void ResetState()
    {
        // estado da rota: volta aos valores iniciais
        targetIndex = 1;
        direction = 1;
        waitTimer = 0f;
        holdFrom = holdTo = 0;
        Velocity = Vector3.zero;

        // posição
        transform.SetPositionAndRotation(startPosition, startRotation);
        body.position = startPosition;
        body.rotation = startRotation;
        Physics.SyncTransforms();
    }

    private void FixedUpdate()
    {
        if (returnWhenInactive)
        {
            ElevatorStep();
            return;
        }

        if (!IsRunning)
        { 
            Velocity = Vector3.zero; 
            return; 
        }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            Velocity = Vector3.zero;
            return;
        }

        Vector3 current = body.position;
        Vector3 next = Vector3.MoveTowards(current, points[targetIndex], speed * Time.fixedDeltaTime);

        Velocity = (next - current) / Time.fixedDeltaTime;
        body.MovePosition(next);

        if (Vector3.Distance(next, points[targetIndex]) < 0.001f) AdvanceTarget();
    }

    // Modo elevador: fonte ativa -> sobe até ao último waypoint; inativa -> desce até ao primeiro
    private void ElevatorStep()
    {
        int goal = IsRunning ? points.Length - 1 : 0;

        // Se o objetivo mudou a meio do caminho, dá meia-volta no mesmo troço
        if (holdTo != holdFrom && Mathf.Abs(goal - holdFrom) < Mathf.Abs(goal - holdTo))
            (holdFrom, holdTo) = (holdTo, holdFrom);

        if ((body.position - points[holdTo]).sqrMagnitude < 0.000001f)
        {
            holdFrom = holdTo;
            if (holdTo == goal)
            {
                Velocity = Vector3.zero; // chegou: fica parado
                return;
            }
            holdTo += goal > holdTo ? 1 : -1;
        }

        Vector3 current = body.position;
        Vector3 next = Vector3.MoveTowards(current, points[holdTo], speed * Time.fixedDeltaTime);
        Velocity = (next - current) / Time.fixedDeltaTime;
        body.MovePosition(next);
    }

    private void AdvanceTarget()
    {
        int candidate = targetIndex + direction;
        if (candidate < 0 || candidate >= points.Length) direction = -direction;
        targetIndex += direction;
        waitTimer = waitTime;
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawWireSphere(waypoints[i].position, 0.2f);
            if (i > 0 && waypoints[i - 1] != null)
                Gizmos.DrawLine(waypoints[i - 1].position, waypoints[i].position);
        }
    }
}