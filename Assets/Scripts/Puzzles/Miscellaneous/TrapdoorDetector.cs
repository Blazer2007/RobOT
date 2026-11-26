using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Avisa o TrapdoorMover que se pode mexer caso os jogadores estiverem dentro da área de aprisionamento
/// </summary>

public class TrapdoorDetector : MonoBehaviour
{
    [SerializeField][UnityEngine.Range(1,2)] private int charactersToDetect = 1;
    private List<GameObject> charactersDetected = new List<GameObject>();
    public event Action OnRequiredCharactersTrapped;
    public event Action<GameObject> OnCharacterEnterRequiredArea;
    private void OnCollisionEnter(Collision collision)
    {
        Collider col = collision.collider;
        if (col.CompareTag("Rob") || col.CompareTag("OT"))
        {
            if (!charactersDetected.Contains(col.gameObject))
                charactersDetected.Add(col.gameObject);
            CheckCharacters(col.gameObject);
        }
        Debug.Log(col);
    }
    private void OnCollisionExit(Collision collision)
    {
        Collider col = collision.collider;
        if (col.CompareTag("Rob") || col.CompareTag("OT"))
        {
            charactersDetected.Remove(col.gameObject);
        }
    }
    private void CheckCharacters(GameObject character) 
    {
        OnCharacterEnterRequiredArea?.Invoke(character);
        if (charactersDetected.Count == charactersToDetect)
            OnRequiredCharactersTrapped?.Invoke();
    }

}
