using System;
using UnityEngine;

public class Lever : MonoBehaviour, IInteractable, IActivationSource, IResetlable
{
    [SerializeField] private string promptOff = "E - Ligar alavanca"; // prompt para ligar/ativar
    [SerializeField] private string promptOn = "E - Desligar alavanca"; // prompt para desligar/desativar
    [SerializeField] private bool canToggleOff = true;   // false = liga só uma vez
    [SerializeField] private bool startActive; // Começa já ativa?
    private Animator handleAnimator; // Animação da alavanca
    public bool IsActive { get; private set; } // Está ativa?
    public event Action<bool> ActivationChanged; // A ativação mudou

    public string Prompt => IsActive ? promptOn : promptOff; // Troca

    private void Awake()
    {
        handleAnimator = GetComponent<Animator>();
        IsActive = startActive;
        UpdateVisual();
    }

    public bool CanInteract(GameObject interactor) => !IsActive || canToggleOff;

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;
        SetActive(!IsActive);
    }

    public void ResetState() => SetActive(startActive);

    private void SetActive(bool value)
    {
        if (value == IsActive) return;
        IsActive = value;
        UpdateVisual();
        ActivationChanged?.Invoke(value);
    }

    private void UpdateVisual()
    {
        if(handleAnimator != null) 
        {
            handleAnimator.SetBool("Activated", IsActive);
        }

    }
}