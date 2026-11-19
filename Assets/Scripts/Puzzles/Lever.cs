using System;
using UnityEngine;

public class Lever : MonoBehaviour, IInteractable, IActivationSource, IResetlable
{
    [SerializeField] private string promptOff = "E - Ligar alavanca";
    [SerializeField] private string promptOn = "E - Desligar alavanca";
    [SerializeField] private bool canToggleOff = true;   // false = liga só uma vez
    [SerializeField] private bool startActive;
    [SerializeField] private Transform handle;           // opcional: pivô visual
    [SerializeField] private float handleAngle = 40f;
    private Animator handleAnimator;
    public bool IsActive { get; private set; }
    public event Action<bool> ActivationChanged;

    public string Prompt => IsActive ? promptOn : promptOff;

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
        if (handle != null && !handleAnimator)
            handle.localRotation = Quaternion.Euler(IsActive ? handleAngle : -handleAngle, 0f, 0f);
        if(handleAnimator != null) 
        {
            handleAnimator.SetBool("Activated", IsActive);
        }

    }
}