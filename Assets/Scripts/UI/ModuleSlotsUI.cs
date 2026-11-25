using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HUD dos módulos: 2 slots principais + 1 slot extra da personagem que estás a controlar.
/// Escuta os eventos do ModuleInventory (ModuleAcquired, ModuleDropped, SelectionChanged) e a
/// troca de personagem (ControlChanged): o inventário não sabe que esta UI existe.
/// Subscrições em OnEnable e anuladas em OnDisable.
/// </summary>
public class ModuleSlotsUI : MonoBehaviour
{
    [SerializeField] private ModuleController[] controllers; // Rob e O.T
    [SerializeField] private ModuleSlotView[] mainSlots;     // 2 slots principais, por ordem
    [SerializeField] private ModuleSlotView extraSlot;

    private readonly List<IControllable> subscribedCharacters = new List<IControllable>();

    private void OnEnable()
    {
        foreach (ModuleController controller in controllers)
        {
            if (controller == null) continue;

            controller.Inventory.ModuleAcquired += OnModuleChanged;
            controller.Inventory.ModuleDropped += OnModuleChanged;
            controller.Inventory.SelectionChanged += OnSelectionChanged;

            if (controller.TryGetComponent(out IControllable character))
            {
                character.ControlChanged += OnControlChanged;
                subscribedCharacters.Add(character);
            }
        }

        Refresh();
    }

    private void OnDisable()
    {
        foreach (ModuleController controller in controllers)
        {
            if (controller == null) continue;

            controller.Inventory.ModuleAcquired -= OnModuleChanged;
            controller.Inventory.ModuleDropped -= OnModuleChanged;
            controller.Inventory.SelectionChanged -= OnSelectionChanged;
        }

        foreach (IControllable character in subscribedCharacters)
            character.ControlChanged -= OnControlChanged;
        subscribedCharacters.Clear();
    }

    // Quem tem o controlo pode só ficar definido depois do OnEnable
    private void Start() => Refresh();

    private void OnModuleChanged(IModuleInfo module) => Refresh();
    private void OnSelectionChanged(int index) => Refresh();
    private void OnControlChanged(bool controlled) => Refresh();

    private void Refresh()
    {
        ModuleInventory inventory = FindControlledInventory();

        for (int i = 0; i < mainSlots.Length; i++)
        {
            ModuleData data = inventory != null ? ToData(inventory.GetMain(i)) : null;
            bool selected = inventory != null && inventory.SelectedIndex == i;
            mainSlots[i].Show(data, selected);
        }

        if (extraSlot != null)
            extraSlot.Show(inventory != null ? ToData(inventory.Extra) : null, false);
    }

    private ModuleInventory FindControlledInventory()
    {
        foreach (ModuleController controller in controllers)
        {
            if (controller != null
                && controller.TryGetComponent(out IControllable character)
                && character.IsControlled)
                return controller.Inventory;
        }
        return null;
    }

    // O inventário guarda IModuleInfo: aqui só interessa chegar aos dados (ícone e nome)
    private static ModuleData ToData(IModuleInfo module)
    {
        if (module is ModulePickup pickup) return pickup.Data;
        return module as ModuleData;
    }
}
