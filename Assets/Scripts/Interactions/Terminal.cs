using System;
using UnityEngine;

public class Terminal : MonoBehaviour, IInteractable, IActivationSource
{
    [SerializeField] private string prompt = "E - Ativar O.T";

    public string Prompt => prompt;
    public bool IsActive { get; private set; }
    public event Action<bool> ActivationChanged;

    public bool CanInteract(GameObject interactor) => !IsActive;

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;
        IsActive = true;
        ActivationChanged?.Invoke(true);
    }
}