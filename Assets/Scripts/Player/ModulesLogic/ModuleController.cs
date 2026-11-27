using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Liga o input e o mundo ao ModuleInventory. Uma instância por personagem (Rob e O.T).
/// Pôr em ControllableCharacter.enableWhileControlled, como o PlayerInteractor.
/// Só conhece IModuleInfo e IRobotAbility: não sabe que módulos existem.
/// </summary>

public class ModuleController : MonoBehaviour, IAirJump
{
    [SerializeField] private Camera aimCamera;          // vazio = Camera.main
    [SerializeField] private Transform abilityRoot;     // pai das habilidades instanciadas; vazio = este objeto
    [SerializeField] private float dropDistance = 1.2f; // largar à frente da personagem
    [SerializeField] private float dropHeight = 0.5f;
    [SerializeField] private float holdToDropExtraTime = 0.6f; // segurar Q para largar o extra
    [SerializeField] private KeyCode dropKey = KeyCode.Q;
    [SerializeField] private KeyCode slot1Key = KeyCode.Alpha1;
    [SerializeField] private KeyCode slot2Key = KeyCode.Alpha2;

    // Público para a UI e os requisitos de portas lerem/escutarem (mesmo com o controlador desligado)
    public ModuleInventory Inventory { get; } = new ModuleInventory();

    private readonly Dictionary<IModuleInfo, IRobotAbility> abilities = new Dictionary<IModuleInfo, IRobotAbility>();
    private AbilityContext context;
    private IRobotAbility activeAbility; // habilidade que está a ser mantida com o clique

    private bool dropKeyHeld;
    private bool extraDropped;
    private float dropHeldTime;

    private void Awake()
    {
        if (aimCamera == null) aimCamera = Camera.main;
        context = new AbilityContext(gameObject, aimCamera);

        // O inventário pertence a este componente (mesmo tempo de vida): subscrever aqui é seguro.
        Inventory.ModuleAcquired += HandleAcquired;
        Inventory.ModuleDropped += HandleDropped;
        Inventory.SelectionChanged += HandleSelectionChanged;
    }

    private void OnDestroy()
    {
        Inventory.ModuleAcquired -= HandleAcquired;
        Inventory.ModuleDropped -= HandleDropped;
        Inventory.SelectionChanged -= HandleSelectionChanged;
    }

    private void OnDisable()
    {
        ReleaseActive();     // perdeu o controlo (Tab) com o clique premido
        dropKeyHeld = false;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return; // jogo em pausa

        HandleSelectionInput();
        HandleUseInput();
        HandleDropInput();
    }

    // ---------- Consultas para a UI (a mira) ----------

    public bool HasSelectedAbility =>
        Inventory.SelectedMain != null && abilities.ContainsKey(Inventory.SelectedMain);

    public AbilityTargetState GetSelectedTargetState()
    {
        if (!HasSelectedAbility) return AbilityTargetState.None;

        IRobotAbility ability = abilities[Inventory.SelectedMain];
        if (ability is ITargetFeedback feedback) return feedback.GetTargetState(context);

        return ability.CanUse(context) ? AbilityTargetState.Available : AbilityTargetState.None;
    }

    // ---------- Salto no ar (IAirJump), pedido pelo PlayerMover ----------
    // Usa a habilidade do módulo EXTRA se for ativada por salto no ar (ex.: propulsor).

    public bool CanAirJump =>
        isActiveAndEnabled && TryGetAirJumpAbility(out IRobotAbility ability) && ability.CanUse(context);

    public void AirJump()
    {
        if (TryGetAirJumpAbility(out IRobotAbility ability) && ability.CanUse(context))
            ability.Use(context);
    }

    private bool TryGetAirJumpAbility(out IRobotAbility ability)
    {
        ability = null;
        return Inventory.Extra is ModulePickup pickup
            && pickup.Data.Activation == ModuleActivation.AirJump
            && abilities.TryGetValue(pickup, out ability);
    }

    // Chamado pelo ModulePickup quando o jogador carrega em E
    public bool TryPickup(ModulePickup pickup)
    {
        Vector3 position = pickup.transform.position;
        Quaternion rotation = pickup.transform.rotation;

        PickupResult result = Inventory.TryPickup(pickup);
        if (!result.Accepted) return false;

        pickup.OnPickedUp();

        // Troca: o módulo antigo fica no sítio de onde saiu o novo
        if (result.Dropped is ModulePickup old)
            old.DropAt(position, rotation);

        return true;
    }

    // ---------- Input ----------

    private void HandleSelectionInput()
    {
        if (Input.GetKeyDown(slot1Key)) Inventory.Select(0);
        if (Input.GetKeyDown(slot2Key)) Inventory.Select(1);
        if (Input.mouseScrollDelta.y != 0f) Inventory.CycleSelection();
    }

    private void HandleUseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            IModuleInfo selected = Inventory.SelectedMain;
            if (selected != null
                && abilities.TryGetValue(selected, out IRobotAbility ability)
                && ability.CanUse(context))
            {
                ability.Use(context);
                activeAbility = ability;
            }
        }

        if (Input.GetMouseButtonUp(0)) ReleaseActive();
    }

    // Q: larga o principal selecionado. Segurar Q: larga o extra.
    private void HandleDropInput()
    {
        if (Input.GetKeyDown(dropKey))
        {
            dropKeyHeld = true;
            extraDropped = false;
            dropHeldTime = 0f;
        }

        if (!dropKeyHeld) return;

        if (Input.GetKey(dropKey))
        {
            dropHeldTime += Time.unscaledDeltaTime;
            if (!extraDropped && Inventory.Extra != null && dropHeldTime >= holdToDropExtraTime)
            {
                extraDropped = true;
                PlaceInFront(Inventory.DropExtra());
            }
        }

        if (Input.GetKeyUp(dropKey))
        {
            if (!extraDropped) PlaceInFront(Inventory.DropSelectedMain());
            dropKeyHeld = false;
        }
    }

    // ---------- Reações aos eventos do inventário ----------

    private void HandleAcquired(IModuleInfo module)
    {
        if (module is not ModulePickup pickup || pickup.Data.AbilityPrefab == null) return;

        GameObject instance = Instantiate(pickup.Data.AbilityPrefab, abilityRoot != null ? abilityRoot : transform);
        if (instance.TryGetComponent<IRobotAbility>(out IRobotAbility ability))
        {
            ability.Setup(pickup.Data);
            abilities[module] = ability;
        }
        else
        {
            Debug.LogError($"{pickup.Data.name}: o AbilityPrefab não tem nenhum componente IRobotAbility.", this);
            Destroy(instance);
        }
    }

    private void HandleDropped(IModuleInfo module)
    {
        if (!abilities.TryGetValue(module, out IRobotAbility ability)) return;

        if (ability == activeAbility) ReleaseActive();
        abilities.Remove(module);

        if (ability is Component component) Destroy(component.gameObject);
    }

    private void HandleSelectionChanged(int index) => ReleaseActive();

    // ---------- Auxiliares ----------

    private void ReleaseActive()
    {
        if (activeAbility == null) return;
        activeAbility.Release();
        activeAbility = null;
    }

    private void PlaceInFront(IModuleInfo module)
    {
        if (module is not ModulePickup pickup) return;

        Vector3 position = transform.position + transform.forward * dropDistance + Vector3.up * dropHeight;
        pickup.DropAt(position, pickup.transform.rotation);
    }
}