using System;
using UnityEngine;

public class ActivationRequirement : MonoBehaviour, IRequirement
{
    [SerializeField] private MonoBehaviour sourceBehaviour;   // vazio = usa o próprio GameObject
    [SerializeField] private bool invert;                      // cumprido quando a fonte está INATIVA

    private IActivationSource source;
    private bool resolved;

    public event Action Changed;

    public bool IsMet => Source != null && (Source.IsActive != invert);

    private IActivationSource Source
    {
        get
        {
            if (!resolved)
            {
                resolved = true;
                source = sourceBehaviour != null
                    ? sourceBehaviour.GetComponent<IActivationSource>()
                    : GetComponent<IActivationSource>();

                if (source == null)
                    Debug.LogError($"{name}: não encontrei nenhuma IActivationSource.", this);
            }
            return source;
        }
    }

    private void OnEnable()
    {
        if (Source != null) Source.ActivationChanged += OnSourceChanged;
    }

    private void OnDisable()
    {
        if (Source != null) Source.ActivationChanged -= OnSourceChanged;
    }

    private void OnSourceChanged(bool active) => Changed?.Invoke();
}