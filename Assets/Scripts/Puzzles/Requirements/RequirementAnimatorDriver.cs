using System.Collections;
using UnityEngine;

/// <summary>
/// Liga requisitos a um parâmetro bool de um Animator: o bool fica true enquanto o grupo
/// de requisitos estiver cumprido. Não move nada (isso é com a Door / ElevatorMover).
/// Reutilizável em qualquer objeto animado (portas da cabine, alavancas, luzes...).
/// </summary>
public class RequirementAnimatorDriver : MonoBehaviour, IResetlable
{
    [SerializeField] private MonoBehaviour[] requirementSources;
    [SerializeField] private GroupMode mode = GroupMode.All;
    [SerializeField] private Animator animator;              // vazio = procura neste objeto ou nos filhos
    [SerializeField] private string boolParameter = "IsOpen";
    [SerializeField] private ElevatorMover elevatorMover;

    private RequirementGroup group;
    private int parameterHash;

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        parameterHash = Animator.StringToHash(boolParameter);
        group = RequirementGroupFactory.Create(requirementSources, mode, this);
        elevatorMover = GetComponent<ElevatorMover>();
    }

    private void OnEnable()
    {
        elevatorMover.ElevatorArrived += OpenDoorsOnArrival;
        group.Changed += Apply;
        group.StartListening();
        Apply(); // estado inicial
    }

    private void OnDisable()
    {
        elevatorMover.ElevatorArrived -= OpenDoorsOnArrival;
        group.StopListening();
        group.Changed -= Apply;
    }
    private void OpenDoorsOnArrival() 
    {
        StartCoroutine(Wait2Seconds());
    }
    private IEnumerator Wait2Seconds() 
    {
        yield return new WaitForSeconds(2f); 
        animator.SetBool(parameterHash,true);
    }
    public void ResetState() => Apply();

    private void Apply()
    {
        if (animator != null) animator.SetBool(parameterHash, group.IsMet);
    }
}
