using UnityEngine;

public class DoorOpener : MonoBehaviour
{
    [SerializeField] private MonoBehaviour sourceBehaviour;   // arrastas a placa no Inspector
    private IActivationSource source;

    private void Awake() 
    { 
        source = sourceBehaviour as IActivationSource; //Faz-se isto, porque não dá para serializar
        // interfaces, então serializa-se um monobehaviour e usa-se ele com a interface desejada
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
