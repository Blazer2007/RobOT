using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Placa de pressão por peso. Soma a massa (Rigidbody.mass) de tudo o que está em cima e
/// ativa quando o total chega a requiredMass. Exemplo com requiredMass = 2:
///   Rob (1)             -> inativa
///   Rob + O.T (1 + 1)   -> ativa
///   Rob + caixa leve    -> ativa (se a caixa pesar >= 1)
/// A massa das caixas vem do ObjectMaterial (densidade x volume), por isso o tamanho e o
/// material da caixa decidem se chega. Uma caixa agarrada pelo braço levita e não pesa.
/// </summary>
public class PressurePlate : MonoBehaviour, IActivationSource, IResetlable
{
    private const float Tolerance = 0.0001f;

    [SerializeField] private LayerMask activators = ~0;
    [SerializeField][Range(0.5f,10f)] private float requiredMass = 1f; // massa total mínima para ativar

    private Animator animator;

    // Rigidbody -> número de colliders desse corpo dentro da placa
    private readonly Dictionary<Rigidbody, int> occupants = new Dictionary<Rigidbody, int>();
    private readonly List<Rigidbody> stale = new List<Rigidbody>();

    public bool IsActive { get; private set; }
    public float CurrentMass { get; private set; }
    public float RequiredMass => requiredMass;

    public event Action<bool> ActivationChanged;

    private void Awake()
    {
        TryGetComponent(out animator);
    }

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

    // Recalcula sempre: apanhar/largar uma caixa com o braço ou esconder um objeto
    // não geram eventos de trigger.
    private void FixedUpdate() => Refresh();

    public void ResetState() => Refresh();

    private void Refresh()
    {
        CurrentMass = ComputeMass();

        bool active = occupants.Count > 0 && CurrentMass >= requiredMass - Tolerance;
        if (active == IsActive) return;

        IsActive = active;
        ActivationChanged?.Invoke(active);
        if (animator != null) animator.SetBool("Activated", active);
    }

    private float ComputeMass()
    {
        stale.Clear();
        float total = 0f;

        foreach (KeyValuePair<Rigidbody, int> pair in occupants)
        {
            Rigidbody rb = pair.Key;

            if (rb == null || !rb.gameObject.activeInHierarchy)
            {
                stale.Add(rb);
                continue;
            }

            // Caixa agarrada pelo braço: está a levitar, não pesa na placa
            if (rb.TryGetComponent(out MaterialObject box) && box.IsGrabbed) continue;

            total += rb.mass;
        }

        foreach (Rigidbody rb in stale) occupants.Remove(rb);
        return total;
    }
}