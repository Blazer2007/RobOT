// O inventário só precisa de saber o nome e em que tipo de slot o módulo cabe.
// Assim depende desta abstração (DIP) e não do ScriptableObject concreto,
// o que permite testá-lo sem assets nem cena.

public enum ModuleSlotType
{
    Main, // Slots principais (2): braço magnético, lanterna, escudo...
    Extra // Slot extra (1): propulsor e futuros objetos extra
}

public interface IModuleInfo
{
    string ModuleName { get; }
    ModuleSlotType SlotType { get; }
}
