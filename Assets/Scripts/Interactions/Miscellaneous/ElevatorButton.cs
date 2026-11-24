using UnityEngine;

/// <summary>
/// Botão do elevador (vai no objeto do botão, onde está o collider que o PlayerInteractor deteta).
/// Só interage; quem se move é o ElevatorMover.
/// </summary>
public class ElevatorButton : MonoBehaviour, IInteractable
{
    [SerializeField] private ElevatorMover elevator;
    [SerializeField] private string prompt = "Ir para o próximo nível";

    public string Prompt => prompt;

    public bool CanInteract(GameObject interactor) => elevator != null && elevator.CanStart;

    public void Interact(GameObject interactor)
    {
        if (CanInteract(interactor)) elevator.StartDescent();
    }
}
