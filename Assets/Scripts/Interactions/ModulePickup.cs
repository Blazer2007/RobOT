// ModulePickup.cs
using UnityEngine;

// O módulo como objeto físico no mundo. É o MESMO objeto desde a sala até ao inventário:
// apanhar = esconder, largar = reaparecer no chão. Por isso o inventário guarda o
// próprio ModulePickup (implementa IModuleInfo) e o reset sabe sempre qual é o original.
// O Collider tem de estar neste mesmo GameObject (o PlayerInteractor procura aqui).
[RequireComponent(typeof(Rigidbody))]
public class ModulePickup : MonoBehaviour, IInteractable, IModuleInfo, IResetlable
{
    [SerializeField] private ModuleData data;
    [SerializeField] private float fallLimitY = -20f; // abaixo disto volta ao sítio original

    private Rigidbody body;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    public ModuleData Data => data;
    public bool IsHeld { get; private set; }

    // IModuleInfo (delegado nos dados)
    public string ModuleName => data.ModuleName;
    public ModuleSlotType SlotType => data.SlotType;

    // IInteractable
    public string Prompt => $"E - Apanhar {data.ModuleName}";

    public bool CanInteract(GameObject interactor) =>
        !IsHeld && interactor.TryGetComponent<ModuleController>(out _);

    public void Interact(GameObject interactor)
    {
        if (interactor.TryGetComponent<ModuleController>(out var controller))
            controller.TryPickup(this);
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        if (!IsHeld && transform.position.y < fallLimitY)
            ReturnToOrigin();
    }

    // Chamado pelo ModuleController
    public void OnPickedUp()
    {
        IsHeld = true;
        StopMoving();
        gameObject.SetActive(false);
    }

    public void DropAt(Vector3 position, Quaternion rotation)
    {
        IsHeld = false;
        gameObject.SetActive(true);
        transform.SetPositionAndRotation(position, rotation);
        body.position = position;
        body.rotation = rotation;
        StopMoving();
        Physics.SyncTransforms();
    }

    // Reiniciar sala: volta ao sítio original, MAS só se estiver no chão.
    // Se alguém o tem no inventário, mantém-se (o inventário não é reposto).
    public void ResetState()
    {
        if (IsHeld) return;
        ReturnToOrigin();
    }

    private void ReturnToOrigin() => DropAt(initialPosition, initialRotation);

    private void StopMoving()
    {
        if (body.isKinematic) return;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
    }
}
