using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Deteta quantas personagens (Rob / O.T) estão dentro da área de aprisionamento.
/// Deve estar num objeto filho da trapdoor com um BoxCollider marcado como "Is Trigger".
/// </summary>
[RequireComponent(typeof(Collider))]
public class TrapdoorDetector : MonoBehaviour, IResetlable
{
    [SerializeField][Range(1, 2)] private int charactersToDetect = 1;

    private readonly HashSet<GameObject> charactersDetected = new HashSet<GameObject>();

    public event Action OnRequiredCharactersTrapped;
    public event Action<GameObject> OnCharacterEnterRequiredArea;

    private void Reset()
    {
        // Garante que o collider é trigger ao adicionar o componente
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject character = GetCharacter(other);
        if (character == null) return;

        // Add devolve false se já estava na lista -> evita chamadas duplicadas
        if (!charactersDetected.Add(character)) return;

        OnCharacterEnterRequiredArea?.Invoke(character);

        if (charactersDetected.Count >= charactersToDetect)
            OnRequiredCharactersTrapped?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject character = GetCharacter(other);
        if (character != null)
            charactersDetected.Remove(character);
    }

    // Devolve o objeto da personagem (Rob ou OT) ou null se não for uma delas
    private static GameObject GetCharacter(Collider col)
    {
        GameObject go = col.attachedRigidbody != null ? col.attachedRigidbody.gameObject : col.gameObject;
        return (go.CompareTag("Rob") || go.CompareTag("OT")) ? go : null;
    }

    // As personagens são teleportadas no reset, por isso o OnTriggerExit pode nunca disparar
    public void ResetState() => charactersDetected.Clear();
}