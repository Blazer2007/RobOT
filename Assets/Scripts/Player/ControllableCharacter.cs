using System;
using UnityEngine;

// Diz se esta personagem está a ser controlada pelo jogador
// e liga/desliga os componentes que só fazem sentido quando está (ex.: PlayerInteractor)
public class ControllableCharacter : MonoBehaviour, IControllable
{
    [SerializeField] private bool startControlled; // true no Rob, false no O.T
    [SerializeField] private Behaviour[] enableWhileControlled; // ex.: PlayerInteractor

    public bool IsControlled { get; private set; }
    public event Action<bool> ControlChanged;

    private void Awake() => Apply(startControlled);

    public void GainControl() => SetControlled(true);
    public void LoseControl() => SetControlled(false);

    private void SetControlled(bool controlled)
    {
        if (controlled == IsControlled) return;
        Apply(controlled);
        ControlChanged?.Invoke(controlled);
    }

    private void Apply(bool controlled)
    {
        IsControlled = controlled;
        foreach (var b in enableWhileControlled)
            if (b != null) b.enabled = controlled;
    }
}
