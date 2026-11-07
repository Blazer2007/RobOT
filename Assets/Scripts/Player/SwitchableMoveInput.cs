using UnityEngine;

// IMoveInput que escolhe a fonte de input conforme a personagem está ou não a ser controlada:
//  - controlada     -> playerSource (teclado, PlayerInput)
//  - não controlada -> aiSource (ex.: FollowInput); se estiver vazio, fica parada
[RequireComponent(typeof(ControllableCharacter))]
public class SwitchableMoveInput : MonoBehaviour, IMoveInput
{
    [SerializeField] private MonoBehaviour playerSource; // PlayerInput
    [SerializeField] private MonoBehaviour aiSource;     // opcional

    private IControllable character;
    private IMoveInput player;
    private IMoveInput ai;

    private IMoveInput Current => character.IsControlled ? player : ai;

    public Vector2 Move => Current != null ? Current.Move : Vector2.zero;
    public bool ConsumeJump() => Current != null && Current.ConsumeJump();

    private void Awake()
    {
        character = GetComponent<IControllable>();
        player = AsInput(playerSource, nameof(playerSource));
        ai = AsInput(aiSource, nameof(aiSource));
    }

    private IMoveInput AsInput(MonoBehaviour source, string field)
    {
        if (source == null) return null;
        if (source == this || source is not IMoveInput input)
        {
            Debug.LogError($"{name}: {field} tem de ser outro componente que implemente IMoveInput.", this);
            return null;
        }
        return input;
    }
}
