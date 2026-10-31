using TMPro;
using UnityEngine;

public class InteractionMessageUI : MonoBehaviour
{
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private TMP_Text label;

    private void OnEnable()
    {
        interactor.FocusChanged += OnFocusChanged;
        OnFocusChanged(null);
    }

    private void OnDisable() => interactor.FocusChanged -= OnFocusChanged;

    private void OnFocusChanged(IInteractable target)
    {
        label.gameObject.SetActive(target != null);
        if (target != null) label.text = target.Prompt;
    }
}