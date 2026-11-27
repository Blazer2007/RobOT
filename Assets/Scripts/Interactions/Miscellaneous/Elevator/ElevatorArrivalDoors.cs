using System.Collections;
using UnityEngine;

/// <summary>
/// Abre as portas da cabine uns segundos depois de o elevador chegar ao destino.
/// Escuta o ElevatorMover; o RequirementAnimatorDriver continua genérico.
/// Se o elevador for reiniciado durante a espera, cancela a abertura pendente.
/// </summary>
public class ElevatorArrivalDoors : MonoBehaviour
{
    [SerializeField] private ElevatorMover elevator;   // vazio = usa o deste objeto
    [SerializeField] private Animator animator;        // vazio = procura neste objeto ou nos filhos
    [SerializeField] private string boolParameter = "IsOpen";
    [SerializeField] private float openDelay = 2f;

    private int parameterHash;
    private Coroutine pending;

    private void Awake()
    {
        if (elevator == null) elevator = GetComponent<ElevatorMover>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        parameterHash = Animator.StringToHash(boolParameter);
    }

    private void OnEnable()
    {
        if (elevator == null)
        {
            Debug.LogError($"{name}: sem ElevatorMover.", this);
            return;
        }

        elevator.ElevatorArrived += OnArrived;
        elevator.ActivationChanged += OnActivationChanged;
    }

    private void OnDisable()
    {
        if (elevator != null)
        {
            elevator.ElevatorArrived -= OnArrived;
            elevator.ActivationChanged -= OnActivationChanged;
        }
        CancelPending();
    }

    private void OnArrived()
    {
        CancelPending();
        pending = StartCoroutine(OpenAfterDelay());
    }

    // false = o elevador foi reiniciado
    private void OnActivationChanged(bool active)
    {
        if (!active) CancelPending();
    }

    private IEnumerator OpenAfterDelay()
    {
        yield return new WaitForSeconds(openDelay);
        pending = null;
        if (animator != null) animator.SetBool(parameterHash, true);
    }

    private void CancelPending()
    {
        if (pending == null) return;
        StopCoroutine(pending);
        pending = null;
    }
}
