using System;
using UnityEngine;

/// <summary>
/// Requisito "alguém tem o módulo X no inventário". Entra no requirementSources de uma Door
/// (ou de qualquer RequirementGroup), em conjunto com placas, alavancas, etc.
/// Cumprido se QUALQUER uma das personagens listadas tiver o módulo (em qualquer slot).
/// Escuta o inventário: a porta reage quando o módulo é apanhado ou largado, sem o
/// inventário saber que este requisito existe.
/// </summary>
public class ModuleRequirement : MonoBehaviour, IRequirement
{
    [SerializeField] private ModuleData requiredModule;
    [SerializeField] private ModuleController[] controllers; // quem conta (Rob e/ou O.T)

    public event Action Changed;

    public bool IsMet
    {
        get
        {
            if (requiredModule == null || controllers == null) return false;

            foreach (ModuleController controller in controllers)
            {
                if (controller != null && controller.Inventory.Contains(m => m.GetData() == requiredModule))
                    return true;
            }
            return false;
        }
    }

    private void Awake()
    {
        if (requiredModule == null || controllers == null || controllers.Length == 0)
            Debug.LogWarning($"{name}: ModuleRequirement sem módulo ou sem personagens.", this);
    }

    private void OnEnable()
    {
        if (controllers == null) return;

        foreach (ModuleController controller in controllers)
        {
            if (controller == null) continue;
            controller.Inventory.ModuleAcquired += OnInventoryChanged;
            controller.Inventory.ModuleDropped += OnInventoryChanged;
        }
    }

    private void OnDisable()
    {
        if (controllers == null) return;

        foreach (ModuleController controller in controllers)
        {
            if (controller == null) continue;
            controller.Inventory.ModuleAcquired -= OnInventoryChanged;
            controller.Inventory.ModuleDropped -= OnInventoryChanged;
        }
    }

    private void OnInventoryChanged(IModuleInfo module) => Changed?.Invoke();
}
