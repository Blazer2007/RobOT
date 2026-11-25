using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Desenha UM slot do HUD: ícone do módulo (ou o nome, se não tiver ícone) e o realce de
/// "selecionado". Só apresentação: não sabe de inventários nem de eventos.
/// </summary>
public class ModuleSlotView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Image selectedFrame; // opcional: realce do slot selecionado
    [SerializeField] private TMP_Text nameLabel;  // opcional: mostra o nome se o módulo não tiver ícone

    // data == null -> slot vazio
    public void Show(ModuleData data, bool selected)
    {
        bool filled = data != null;
        bool hasIcon = filled && data.Icon != null;

        if (icon != null)
        {
            icon.enabled = hasIcon;
            if (hasIcon) icon.sprite = data.Icon;
        }

        if (nameLabel != null)
            nameLabel.text = filled && !hasIcon ? data.ModuleName : string.Empty;

        if (selectedFrame != null)
            selectedFrame.enabled = selected;
    }
}
