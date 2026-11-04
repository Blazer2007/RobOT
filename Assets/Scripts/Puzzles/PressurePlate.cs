using System;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour, IActivationSource
{
    [SerializeField] private LayerMask activators = ~0;

    private readonly Dictionary<Rigidbody, int> occupants = new Dictionary<Rigidbody, int>();

    public bool IsActive { get; private set; }
    public event Action<bool> ActivationChanged;

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null || (activators.value & (1 << other.gameObject.layer)) == 0) return;

        occupants[rb] = occupants.TryGetValue(rb, out int count) ? count + 1 : 1;
        Refresh();
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null || !occupants.TryGetValue(rb, out int count)) return;

        if (count <= 1) occupants.Remove(rb);
        else occupants[rb] = count - 1;
        Refresh();
    }

    private void Refresh()
    {
        bool active = occupants.Count > 0;
        if (active == IsActive) return;
        IsActive = active;
        ActivationChanged?.Invoke(active);
    }
}