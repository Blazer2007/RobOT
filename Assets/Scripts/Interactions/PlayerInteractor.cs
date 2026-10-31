using UnityEngine;
using System;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float interactRadius = 2f;
    [SerializeField, Range(0f, 1f)] private float minFacingDot = 0.3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private LayerMask interactableLayers = ~0;

    private readonly Collider[] buffer = new Collider[16];
    private IInteractable current;

    /// Emitido quando o alvo em foco muda (null = nenhum). A UI escuta isto.
    public event Action<IInteractable> FocusChanged;

    private void Update()
    {
        SetFocus(FindBestTarget());

        if (current != null && Input.GetKeyDown(interactKey))
            current.Interact(gameObject);
    }

    private void OnDisable() => SetFocus(null);

    private IInteractable FindBestTarget()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, interactRadius, buffer,
            interactableLayers, QueryTriggerInteraction.Collide);

        IInteractable best = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            if (!buffer[i].TryGetComponent<IInteractable>(out var candidate)) continue;
            if (!candidate.CanInteract(gameObject)) continue;

            Vector3 toTarget = buffer[i].bounds.center - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > 0.0001f &&
                Vector3.Dot(transform.forward, toTarget.normalized) < minFacingDot) continue;

            float distance = toTarget.sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = candidate;
            }
        }
        return best;
    }

    private void SetFocus(IInteractable target)
    {
        if (ReferenceEquals(target, current)) return;
        current = target;
        FocusChanged?.Invoke(current);
    }
}
