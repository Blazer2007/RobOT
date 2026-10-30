using UnityEngine;

public interface IInteractable
{
    bool CanInteract(GameObject interactor) 
    {
        bool canInteract = false;
        return canInteract;
    }
    void Interact(GameObject interactor) 
    {
        
    }
}
