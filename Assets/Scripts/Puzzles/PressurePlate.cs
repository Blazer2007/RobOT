using System;
using UnityEngine;

public class PressurePlate : MonoBehaviour, IActivationSource
{
    public bool IsActive { get; private set; }
    public event Action<bool> ActivationChanged;

    private void SetActive(bool value)
    {
        if (IsActive == value) return;
        IsActive = value;
        ActivationChanged?.Invoke(value);
    }
    // OnTriggerEnter/Exit chamam SetActive(true/false)
}