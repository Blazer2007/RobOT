using System;
using UnityEngine;

public class RobotPowerUp : MonoBehaviour
{
    [SerializeField] private MonoBehaviour sourceBehaviour;      // arrastar o Terminal
    [SerializeField] private Behaviour[] enableOnPowerUp;        // scripts do O.T a ligar

    private IActivationSource source;

    public bool IsPowered { get; private set; }
    public event Action PoweredOn;

    private void Awake()
    {
        source = sourceBehaviour as IActivationSource;
        if (source == null)
            Debug.LogError($"{name}: a fonte atribuída não implementa IActivationSource.", this);

        foreach (var b in enableOnPowerUp) b.enabled = false;
    }

    private void OnEnable()
    {
        if (source != null) source.ActivationChanged += OnSourceChanged;
    }

    private void OnDisable()
    {
        if (source != null) source.ActivationChanged -= OnSourceChanged;
    }

    private void OnSourceChanged(bool active)
    {
        if (!active || IsPowered) return;
        IsPowered = true;
        foreach (var b in enableOnPowerUp) b.enabled = true;
        PoweredOn?.Invoke();
    }
}