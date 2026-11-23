using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Mira no centro do ecrã. Só aparece quando a personagem controlada tem um módulo
/// principal com habilidade selecionado, e muda de cor conforme o alvo:
/// None -> branca | Available -> verde | Blocked -> vermelha | Active -> ciano
/// Pôr no Image da mira (centrado no Canvas) e arrastar os ModuleController do Rob e do O.T.
/// </summary>

public class CrosshairUI : MonoBehaviour
{
    [SerializeField] private ModuleController[] controllers; // Rob e O.T
    [SerializeField] private Image image;

    [SerializeField] private Color noneColor = new Color(1f, 1f, 1f, 0.6f);
    [SerializeField] private Color availableColor = new Color(0.3f, 1f, 0.4f, 1f);
    [SerializeField] private Color blockedColor = new Color(1f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color activeColor = new Color(0.3f, 0.9f, 1f, 1f);

    private void Reset() => image = GetComponent<Image>();

    private void Update()
    {
        ModuleController current = FindControlled();
        bool visible = current != null && current.HasSelectedAbility && Time.timeScale != 0f;

        image.enabled = visible;
        if (visible) image.color = ColorFor(current.GetSelectedTargetState());
    }

    private ModuleController FindControlled()
    {
        foreach (ModuleController controller in controllers)
        {
            if (controller != null
                && controller.TryGetComponent<IControllable>(out IControllable character)
                && character.IsControlled)
                return controller;
        }
        return null;
    }

    private Color ColorFor(AbilityTargetState state)
    {
        switch (state)
        {
            case AbilityTargetState.Available: return availableColor;
            case AbilityTargetState.Blocked: return blockedColor;
            case AbilityTargetState.Active: return activeColor;
            default: return noneColor;
        }
    }
}