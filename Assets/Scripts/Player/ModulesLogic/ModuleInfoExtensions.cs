/// <summary>
/// O inventário guarda IModuleInfo (na prática, o ModulePickup do objeto físico).
/// Isto chega aos dados (ModuleData) sem repetir o cast em cada script (HUD, requisitos...).
/// </summary>
public static class ModuleInfoExtensions
{
    public static ModuleData GetData(this IModuleInfo module)
    {
        if (module is ModulePickup pickup) return pickup.Data;
        return module as ModuleData;
    }
}
