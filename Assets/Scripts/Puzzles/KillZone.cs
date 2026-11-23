using UnityEngine;

// Zona de morte (ex.: fundo de um buraco). Se uma personagem (Rob ou O.T) entrar no trigger,
// "morre" e volta ao ponto de respawn, sem velocidade.
[RequireComponent(typeof(Collider))]
public class KillZone : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint; // ex.: o SpawnRob da sala

    // Ao adicionar o componente, o collider passa logo a trigger
    private void Reset() => GetComponent<Collider>().isTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null || respawnPoint == null) return;

        // Só personagens (as portas e plataformas também têm ResettableTransform)
        if (!rb.TryGetComponent(out ControllableCharacter _)) return;
        if (!rb.TryGetComponent(out ResettableTransform resettable)) return;

        resettable.MoveTo(respawnPoint.position, respawnPoint.rotation);
        Debug.Log($"{rb.name} caiu e voltou ao respawn.", this);
    }
}
