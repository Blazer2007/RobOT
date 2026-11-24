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

    private RequirementGroup group;
    private int parameterHash;

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        parameterHash = Animator.StringToHash(boolParameter);
        group = RequirementGroupFactory.Create(requirementSources, mode, this);
    }

    private void OnEnable()
    {
        group.Changed += Apply;
        group.StartListening();
        Apply(); // estado inicial
    }

    private void OnDisable()
    {
        group.StopListening();
        group.Changed -= Apply;
    }

    public void ResetState() => Apply();

    private void Apply()
    {
        if (animator != null) animator.SetBool(parameterHash, group.IsMet);
    }
}
