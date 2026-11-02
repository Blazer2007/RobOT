using UnityEngine;
using System;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float interactRadius = 2f; // Raio da esfera de interação com os objetos
    [SerializeField, Range(-1f, 1f)] private float minFacingDot = 0.3f; // Raio de visão do jogador
    [SerializeField] private KeyCode interactKey = KeyCode.E; // Tecla de interação
    [SerializeField] private LayerMask interactableLayers = ~0; // Layers interagiveis, "~" inverte o bit a seguir(~0 = todas)

    private readonly Collider[] buffer = new Collider[16]; // Cria um máximo de colliders a serem vistos(se houver mais são ignorados)
    private IInteractable current; // Objeto interagivel atual (para o qual estamos a olhar)

    /// Emitido quando o alvo em foco muda (null = nenhum). A UI escuta isto.
    public event Action<IInteractable> FocusChanged;

    private void Update()
    {
        SetFocus(FindBestTarget()); // Procurar  o melhor alvo de foco de interação

        // Se o objeto(IInteractable) atual existir e se clicar na interactKey(E), interage com esse objeto
        if (current != null && Input.GetKeyDown(interactKey))
            current.Interact(gameObject);
    }

    private void OnDisable() => SetFocus(null); // Se o jogador estiver desativado/eliminado tira-se o foco de qualquer objeto

    private IInteractable FindBestTarget() //Encontrar o melhor candidato de interação
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, interactRadius, buffer,
            interactableLayers, QueryTriggerInteraction.Collide); // Guarda quantos colliders estão dentro do raio de interação

        IInteractable best = null; // Iniciar a variavel do melhor candidato de interação com null
        float bestDistance = float.MaxValue; // Iniciar a distancia do melhor candidato como a máxima

        for (int i = 0; i < count; i++)
        {
            // Verifica se os objetos têm o componente IInteractable,
            // guarda os que tiverem esse componente na variavel candidate e se não,
            // continua a verificar os outros colliders (vai passando por todos os i(objetos dentro do interactRadius))
            if (!buffer[i].TryGetComponent<IInteractable>(out var candidate)) continue;

            // Verifica se se pode interagir com o objeto, se não der continua a verificar os outros colliders
            if (!candidate.CanInteract(gameObject)) continue;

            // Guarda o vetor(distância) entre o jogador e o objeto atual com IInteractable
            Vector3 toTarget = buffer[i].bounds.center - transform.position;
            toTarget.y = 0f; // Ignora o Y ( mantem a altura do vetor uniforme)

            // ????????????????????????????????????????????????????????????????????????????
            if (toTarget.sqrMagnitude > 0.0001f &&
                Vector3.Dot(transform.forward, toTarget.normalized) < minFacingDot) continue;

            // Guarda a distância entre o jogador e o objeto interagivel(ao quadrado)
            float distance = toTarget.sqrMagnitude;
            //Verifica se a distância atual entre os objetos é menor que a melhor distância...
            if (distance < bestDistance)
            {
                // se for menor então a melhor distância passa a ser a distância atual...
                bestDistance = distance;
                // e o melhor candidato passa a ser o candidato que está a essa distância
                best = candidate;
            }
        }
        return best; // devolver ao método o melhor candidato para interação
    }

    private void SetFocus(IInteractable target) // Definir que objeto é o prioritário para interagir
    {
        if (ReferenceEquals(target, current)) return; // se o melhor candidato for o objeto atual não se faz nada
        // Se prosseguir(o objeto alvo mudou)...
        current = target; // O objeto atual passa a ser o melhor candidato(alvo)
        FocusChanged?.Invoke(current); // Indica que o foco mudou para o objeto atual
        // Diz a todos os que estão à escuta(para eles responderem de acordo com a troca de objeto)
    }

    //Este método desenha o raio de interação à volta do jogador
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
