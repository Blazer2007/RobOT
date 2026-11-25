using UnityEngine;

/// <summary>
/// Botão do elevador (vai no objeto do botão, onde está o collider que o PlayerInteractor deteta).
/// Só interage; quem se move é o ElevatorMover.
/// </summary>
public class ElevatorButton : MonoBehaviour, IInteractable
{
    [SerializeField] private ElevatorMover elevator;
    [SerializeField] private string normalPrompt = "Ir para o próximo nível";
    [SerializeField] private string missingCharactersPrompt = "Falta um personagem";
    [SerializeField] private CheckPlayerCount pChecker;
    public string Prompt => allInside ? normalPrompt : missingCharactersPrompt;
    
    private bool allInside;
    void Awake() 
    {
        if (!pChecker)
            Debug.Log("Verificador de jogadores em falta");
    }
    private void OnEnable()
    {
        pChecker.AllCharactersInside += AllCharactersInside;
    }
    private void OnDisable()
    {
        pChecker.AllCharactersInside -= AllCharactersInside;
    }
    private void AllCharactersInside(bool allCharInside) 
    {
        allInside = allCharInside;
        if (!allCharInside)
            Debug.Log("Faltam Personagens");
    }
    public bool CanInteract(GameObject interactor) => elevator != null && elevator.CanStart;

    public void Interact(GameObject interactor)
    {
        if (CanInteract(interactor) && allInside) elevator.StartDescent();
    }
}
