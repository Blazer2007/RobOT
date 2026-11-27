using System;

// Resultado de tentar apanhar um módulo.
// Dropped é o módulo que ficou sem lugar (troca) e tem de ser largado no mundo.
public readonly struct PickupResult
{
    public bool Accepted { get; }
    public IModuleInfo Dropped { get; }

    public PickupResult(bool accepted, IModuleInfo dropped)
    {
        Accepted = accepted;
        Dropped = dropped;
    }
}

// Regras do inventário de UMA personagem: 2 slots principais + 1 slot extra.
// Classe C# pura (sem MonoBehaviour): não cria objetos, só decide e avisa.
// Quem cria/destrói objetos no mundo é o ModuleController.
public class ModuleInventory
{
    public const int MainSlotCount = 2;

    private readonly IModuleInfo[] mainSlots = new IModuleInfo[MainSlotCount];

    public IModuleInfo Extra { get; private set; }
    public int SelectedIndex { get; private set; }
    public IModuleInfo SelectedMain => mainSlots[SelectedIndex];

    public event Action<IModuleInfo> ModuleAcquired;
    public event Action<IModuleInfo> ModuleDropped;
    public event Action<int> SelectionChanged;

    public IModuleInfo GetMain(int index)
    {
        return IsValidIndex(index) ? mainSlots[index] : null;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="module"></param>
    /// <returns></returns>
    public bool Has(IModuleInfo module)
    {
        if (module == null) return false;
        if (Extra == module) return true;
        for (int i = 0; i < MainSlotCount; i++)
            if (mainSlots[i] == module) return true;
        return false;
    }

    /// <summary>
    /// Há algum módulo (em qualquer slot) que cumpra a condição? Usado por requisitos de portas,
    /// que procuram "o módulo X" sem o inventário saber o que é um ModuleData.
    /// </summary>
    public bool Contains(Func<IModuleInfo, bool> match)
    {
        if (match == null) return false;
        if (Extra != null && match(Extra)) return true;
        for (int i = 0; i < MainSlotCount; i++)
            if (mainSlots[i] != null && match(mainSlots[i])) return true;
        return false;
    }

    public PickupResult TryPickup(IModuleInfo module)
    {
        if (module == null) return new PickupResult(false, null);

        return module.SlotType == ModuleSlotType.Extra
            ? PickupExtra(module)
            : PickupMain(module);
    }

    // Q: larga o slot principal selecionado. Devolve null se estiver vazio.
    public IModuleInfo DropSelectedMain()
    {
        IModuleInfo dropped = mainSlots[SelectedIndex];
        if (dropped == null) return null;

        mainSlots[SelectedIndex] = null;
        ModuleDropped?.Invoke(dropped);
        return dropped;
    }

    // Segurar Q: larga o módulo extra. Devolve null se estiver vazio.
    public IModuleInfo DropExtra()
    {
        IModuleInfo dropped = Extra;
        if (dropped == null) return null;

        Extra = null;
        ModuleDropped?.Invoke(dropped);
        return dropped;
    }

    public bool Select(int index)
    {
        if (!IsValidIndex(index)) return false;
        if (index == SelectedIndex) return true;

        SelectedIndex = index;
        SelectionChanged?.Invoke(SelectedIndex);
        return true;
    }

    public void CycleSelection()
    {
        Select((SelectedIndex + 1) % MainSlotCount);
    }

    private PickupResult PickupMain(IModuleInfo module)
    {
        int free = FindFreeMainSlot();

        if (free >= 0)
        {
            mainSlots[free] = module;
            Select(free); // o que acabaste de apanhar fica pronto a usar
            ModuleAcquired?.Invoke(module);
            return new PickupResult(true, null);
        }

        // Slots cheios: troca com o selecionado
        IModuleInfo old = mainSlots[SelectedIndex];
        mainSlots[SelectedIndex] = module;
        ModuleDropped?.Invoke(old);
        ModuleAcquired?.Invoke(module);
        return new PickupResult(true, old);
    }

    private PickupResult PickupExtra(IModuleInfo module)
    {
        IModuleInfo old = Extra;
        Extra = module;

        if (old != null) ModuleDropped?.Invoke(old);
        ModuleAcquired?.Invoke(module);
        return new PickupResult(true, old);
    }

    private int FindFreeMainSlot()
    {
        for (int i = 0; i < MainSlotCount; i++)
            if (mainSlots[i] == null) return i;
        return -1;
    }

    private static bool IsValidIndex(int index)
    {
        return index >= 0 && index < MainSlotCount;
    }
}