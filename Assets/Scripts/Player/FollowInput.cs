using System;
using UnityEngine;
using UnityEngine.AI;

// IMoveInput de IA: segue um alvo (o Rob) usando um caminho calculado na NavMesh.
// Não usa NavMeshAgent: quem move a personagem continua a ser o PlayerMover (Rigidbody),
// este script só diz para onde andar. O Move está em coordenadas do mundo (x, z).
public class FollowInput : MonoBehaviour, IMoveInput
{
    [SerializeField] private Transform target;              // o Rob
    [SerializeField] private float stopDistance = 2f;       // pára a esta distância do alvo
    [SerializeField] private float slowDownDistance = 1.5f; // abranda nos últimos metros antes de parar
    [SerializeField] private float repathInterval = 0.25f;  // de quanto em quanto tempo recalcula o caminho
    [SerializeField] private float sampleRadius = 2f;       // distância máxima para "encaixar" uma posição na NavMesh
    [SerializeField] private KeyCode toggleKey = KeyCode.F; // liga/desliga o seguimento
    [SerializeField] private bool startFollowing = true;

    private NavMeshPath path;
    private int corner;      // próximo canto do caminho a alcançar
    private float nextRepathTime;
    private IControllable character; // para saber se o O.T está a ser controlado

    public Vector2 Move { get; private set; }
    public bool ConsumeJump() => false; // por agora o O.T não salta sozinho

    public bool IsFollowing { get; private set; }
    public event Action<bool> FollowingChanged; // para a UI, sons, etc.

    private void Awake()
    {
        path = new NavMeshPath();
        character = GetComponent<IControllable>();
        IsFollowing = startFollowing;
    }

    private void OnDisable() => Move = Vector2.zero;

    public void SetFollowing(bool follow)
    {
        if (follow == IsFollowing) return;
        IsFollowing = follow;
        if (!follow) Move = Vector2.zero;
        Debug.Log($"{name}: {(follow ? "a seguir" : "fica aqui")}.", this);
        FollowingChanged?.Invoke(follow);
    }

    private void Update()
    {
        // A tecla só conta quando estás a jogar com a outra personagem (é uma ordem do Rob ao O.T)
        bool controlled = character != null && character.IsControlled;
        if (!controlled && Input.GetKeyDown(toggleKey)) SetFollowing(!IsFollowing);

        if (!IsFollowing || target == null) { Move = Vector2.zero; return; }

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;

        if (distance <= stopDistance) { Move = Vector2.zero; return; }

        if (Time.time >= nextRepathTime)
        {
            nextRepathTime = Time.time + repathInterval;
            RecalculatePath();
        }

        Vector3 dir = NextDirection(toTarget);
        float speed = Mathf.Clamp01((distance - stopDistance) / Mathf.Max(slowDownDistance, 0.01f));
        Move = new Vector2(dir.x, dir.z) * speed;
    }

    private void RecalculatePath()
    {
        corner = 1; // o canto 0 é a posição inicial
        // As personagens podem abrir um "buraco" na NavMesh, por isso encaixamos as posições na mais próxima
        if (!NavMesh.SamplePosition(transform.position, out var from, sampleRadius, NavMesh.AllAreas) ||
            !NavMesh.SamplePosition(target.position, out var to, sampleRadius, NavMesh.AllAreas) ||
            !NavMesh.CalculatePath(from.position, to.position, NavMesh.AllAreas, path))
            path.ClearCorners();
    }

    private Vector3 NextDirection(Vector3 toTarget)
    {
        // Avança os cantos que já foram alcançados
        while (corner < path.corners.Length)
        {
            Vector3 toCorner = path.corners[corner] - transform.position;
            toCorner.y = 0f;
            if (toCorner.sqrMagnitude > 0.25f) return toCorner.normalized; // ainda não chegou (0.5 m)
            corner++;
        }
        // Sem caminho (ex.: NavMesh não feita): vai em linha reta
        return toTarget.normalized;
    }

    private void OnDrawGizmosSelected()
    {
        if (path == null || path.corners.Length < 2) return;
        Gizmos.color = Color.yellow;
        for (int i = 1; i < path.corners.Length; i++)
            Gizmos.DrawLine(path.corners[i - 1], path.corners[i]);
    }
}