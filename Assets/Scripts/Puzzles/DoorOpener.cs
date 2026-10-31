using UnityEngine;

public class DoorOpener : MonoBehaviour
{
    [SerializeField] private MonoBehaviour sourceBehaviour;   // arrastas a placa no Inspector
    private IActivationSource source;

    private void Awake() 
    { 
        source = sourceBehaviour as IActivationSource; 
    }
    private void OnEnable()
    {
        source.ActivationChanged += OnActivationChanged;
    }
    private void OnDisable() 
    { 
        source.ActivationChanged -= OnActivationChanged;
    }

    private void OnActivationChanged(bool active) 
    { 
        /* abre ou fecha a porta */ 
    }
}
