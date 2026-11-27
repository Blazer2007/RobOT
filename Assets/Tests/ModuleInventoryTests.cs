using System.Collections.Generic;
using NUnit.Framework;

public class ModuleInventoryTests
{
    // Módulo falso: prova que o inventário só depende de IModuleInfo
    private class FakeModule : IModuleInfo
    {
        public string ModuleName { get; }
        public ModuleSlotType SlotType { get; }

        public FakeModule(string name, ModuleSlotType type = ModuleSlotType.Main)
        {
            ModuleName = name;
            SlotType = type;
        }
    }

    private ModuleInventory inventory;
    private FakeModule arm, lantern, shield, thruster, hook;

    [SetUp]
    public void SetUp()
    {
        inventory = new ModuleInventory();
        arm = new FakeModule("Braço");
        lantern = new FakeModule("Lanterna");
        shield = new FakeModule("Escudo");
        thruster = new FakeModule("Propulsor", ModuleSlotType.Extra);
        hook = new FakeModule("Gancho", ModuleSlotType.Extra);
    }

    // --- Apanhar principais ---

    [Test]
    public void PickupMain_EmptyInventory_GoesToFirstSlotAndIsSelected()
    {
        PickupResult result = inventory.TryPickup(arm);

        Assert.IsTrue(result.Accepted);
        Assert.IsNull(result.Dropped);
        Assert.AreSame(arm, inventory.GetMain(0));
        Assert.AreSame(arm, inventory.SelectedMain);
    }

    [Test]
    public void PickupMain_SecondModule_GoesToFreeSlotAndBecomesSelected()
    {
        inventory.TryPickup(arm);
        inventory.TryPickup(lantern);

        Assert.AreSame(arm, inventory.GetMain(0));
        Assert.AreSame(lantern, inventory.GetMain(1));
        Assert.AreEqual(1, inventory.SelectedIndex);
    }

    [Test]
    public void PickupMain_SlotsFull_SwapsWithSelectedAndReturnsOldOne()
    {
        inventory.TryPickup(arm);
        inventory.TryPickup(lantern);
        inventory.Select(0); // braço selecionado

        PickupResult result = inventory.TryPickup(shield);

        Assert.IsTrue(result.Accepted);
        Assert.AreSame(arm, result.Dropped);
        Assert.AreSame(shield, inventory.GetMain(0));
        Assert.AreSame(lantern, inventory.GetMain(1));
    }

    [Test]
    public void PickupNull_IsRejected()
    {
        PickupResult result = inventory.TryPickup(null);

        Assert.IsFalse(result.Accepted);
    }

    // --- Slot extra ---

    [Test]
    public void PickupExtra_DoesNotUseMainSlots()
    {
        inventory.TryPickup(thruster);

        Assert.AreSame(thruster, inventory.Extra);
        Assert.IsNull(inventory.GetMain(0));
        Assert.IsNull(inventory.GetMain(1));
    }

    [Test]
    public void PickupExtra_SlotOccupied_SwapsAndReturnsOldOne()
    {
        inventory.TryPickup(thruster);

        PickupResult result = inventory.TryPickup(hook);

        Assert.AreSame(thruster, result.Dropped);
        Assert.AreSame(hook, inventory.Extra);
    }

    [Test]
    public void PickupMain_DoesNotAffectExtra()
    {
        inventory.TryPickup(thruster);
        inventory.TryPickup(arm);
        inventory.TryPickup(lantern);
        inventory.TryPickup(shield);

        Assert.AreSame(thruster, inventory.Extra);
    }

    // --- Largar (Q) ---

    [Test]
    public void DropSelectedMain_EmptiesSlotAndReturnsModule()
    {
        inventory.TryPickup(arm);

        IModuleInfo dropped = inventory.DropSelectedMain();

        Assert.AreSame(arm, dropped);
        Assert.IsNull(inventory.SelectedMain);
        Assert.IsFalse(inventory.Has(arm));
    }

    [Test]
    public void DropSelectedMain_EmptySlot_ReturnsNullAndRaisesNothing()
    {
        int events = 0;
        inventory.ModuleDropped += _ => events++;

        IModuleInfo dropped = inventory.DropSelectedMain();

        Assert.IsNull(dropped);
        Assert.AreEqual(0, events);
    }

    [Test]
    public void DropExtra_EmptiesExtraAndLeavesMainSlotsAlone()
    {
        inventory.TryPickup(arm);
        inventory.TryPickup(thruster);

        IModuleInfo dropped = inventory.DropExtra();

        Assert.AreSame(thruster, dropped);
        Assert.IsNull(inventory.Extra);
        Assert.AreSame(arm, inventory.SelectedMain);
    }

    // --- Seleção ---

    [Test]
    public void Select_InvalidIndex_IsIgnored()
    {
        inventory.TryPickup(arm);

        Assert.IsFalse(inventory.Select(5));
        Assert.IsFalse(inventory.Select(-1));
        Assert.AreEqual(0, inventory.SelectedIndex);
    }

    [Test]
    public void CycleSelection_WrapsAround()
    {
        inventory.CycleSelection();
        Assert.AreEqual(1, inventory.SelectedIndex);

        inventory.CycleSelection();
        Assert.AreEqual(0, inventory.SelectedIndex);
    }

    // --- Eventos ---

    [Test]
    public void Pickup_RaisesModuleAcquiredOnce()
    {
        var acquired = new List<IModuleInfo>();
        inventory.ModuleAcquired += acquired.Add;

        inventory.TryPickup(arm);

        Assert.AreEqual(1, acquired.Count);
        Assert.AreSame(arm, acquired[0]);
    }

    [Test]
    public void Swap_RaisesDroppedThenAcquired()
    {
        inventory.TryPickup(arm);
        inventory.TryPickup(lantern);
        inventory.Select(0);

        var order = new List<string>();
        inventory.ModuleDropped += m => order.Add("drop:" + m.ModuleName);
        inventory.ModuleAcquired += m => order.Add("get:" + m.ModuleName);

        inventory.TryPickup(shield);

        CollectionAssert.AreEqual(new[] { "drop:Braço", "get:Escudo" }, order);
    }

    [Test]
    public void SelectionChanged_RaisedOnlyWhenIndexChanges()
    {
        int events = 0;
        inventory.SelectionChanged += _ => events++;

        inventory.Select(0); // já era 0
        inventory.Select(1);

        Assert.AreEqual(1, events);
    }

    // --- Consulta (usada por requisitos de portas) ---

    [Test]
    public void Has_ReportsModulesInAnySlot()
    {
        inventory.TryPickup(arm);
        inventory.TryPickup(thruster);

        Assert.IsTrue(inventory.Has(arm));
        Assert.IsTrue(inventory.Has(thruster));
        Assert.IsFalse(inventory.Has(shield));
        Assert.IsFalse(inventory.Has(null));
    }

    // --- Procura por condição (usada pelo ModuleRequirement) ---

    [Test]
    public void Contains_FindsModuleByCondition()
    {
        inventory.TryPickup(arm);
        inventory.TryPickup(thruster);

        Assert.IsTrue(inventory.Contains(m => m.ModuleName == "Propulsor"));
        Assert.IsTrue(inventory.Contains(m => m.SlotType == ModuleSlotType.Extra));
        Assert.IsFalse(inventory.Contains(m => m.ModuleName == "Escudo"));
    }

    [Test]
    public void Contains_EmptyInventory_ReturnsFalse()
    {
        Assert.IsFalse(inventory.Contains(m => true));
    }

    [Test]
    public void Contains_AfterDrop_NoLongerFindsModule()
    {
        inventory.TryPickup(arm);
        inventory.DropSelectedMain();

        Assert.IsFalse(inventory.Contains(m => m.ModuleName == "Braço"));
    }

    [Test]
    public void Contains_NullCondition_ReturnsFalse()
    {
        inventory.TryPickup(arm);

        Assert.IsFalse(inventory.Contains(null));
    }
}