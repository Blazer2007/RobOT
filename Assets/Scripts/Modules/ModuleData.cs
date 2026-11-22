using UnityEngine;

// Como o módulo é ativado
public enum ModuleActivation
{
    Click,   // Clique esquerdo (módulo principal selecionado)
    AirJump  // Salto no ar (propulsor)
}

// Só dados de configuração. O estado de execução (cooldown atual, objeto agarrado)
// fica nos componentes, porque este asset é partilhado por todas as instâncias.
[CreateAssetMenu(menuName = "RobOT/Module Data", fileName = "ModuleData")]
public class ModuleData : ScriptableObject, IModuleInfo
{
    [field: SerializeField] public string ModuleName { get; private set; } = "Módulo";
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public ModuleSlotType SlotType { get; private set; } = ModuleSlotType.Main;
    [field: SerializeField] public ModuleActivation Activation { get; private set; } = ModuleActivation.Click;

    // Parâmetros das habilidades
    [field: SerializeField] public float Range { get; private set; } = 5f;
    [field: SerializeField] public float Force { get; private set; } = 10f;
    [field: SerializeField] public float Cooldown { get; private set; } = 0f;
    [field: SerializeField] public float MaxMass { get; private set; } = 10f; // massa máxima que consegue levantar (braço)

    // Prefab com o componente IRobotAbility; o ModuleController instancia-o ao equipar.
    // Pode ficar vazio (o módulo apanha-se e larga-se, mas não faz nada).
    [field: SerializeField] public GameObject AbilityPrefab { get; private set; }
}