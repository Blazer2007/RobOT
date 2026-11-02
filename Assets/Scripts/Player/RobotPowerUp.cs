using System;
using UnityEngine;

public class RobotPowerUp : MonoBehaviour
{
    [SerializeField] private MonoBehaviour sourceBehaviour;
    [SerializeField] private Behaviour[] enableOnPowerUp; //Lista de componentes do O.T                                         

    private IActivationSource source;

    public bool IsPowered { get; private set; }
    public event Action PoweredOn;

    private void Awake()
    {
        source = sourceBehaviour as IActivationSource; //Referenciar o interface do sourceBehaviour
        if (source == null)
            Debug.LogError($"{name}: a fonte atribuída não implementa IActivationSource.", this);

        foreach (var b in enableOnPowerUp) b.enabled = false; //desativar todos os componentes do OT
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
        if (!active || IsPowered) return; // se não estiver ativo ou estiver ligado não faz nada
        // Caso contrário...
        IsPowered = true; // Indica que passou a estar ligado
        foreach (var b in enableOnPowerUp) b.enabled = true; // Liga os componentes do O.T
        PoweredOn?.Invoke(); // Diz a todos os que estão à escuta que o OT está ligado
    }
}