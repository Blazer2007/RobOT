using System;
using System.Collections.Generic;
using UnityEngine;

public class CheckPlayerCount : MonoBehaviour
{
    [SerializeField] private int totalCharacters = 2;
    private List<GameObject> charactersInside = new List<GameObject>();
    [HideInInspector] public bool areAllCharactersInside = false;
    public event Action<bool> OnAllCharactersInside;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Rob") || other.CompareTag("OT"))
        {
            if (!charactersInside.Contains(other.gameObject))
            {
                charactersInside.Add(other.gameObject);
            }

            CheckCharacters();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Rob") || other.CompareTag("OT"))
        {
            charactersInside.Remove(other.gameObject);

            CheckCharacters();
        }
    }

    private void CheckCharacters()
    {
        areAllCharactersInside = charactersInside.Count >= totalCharacters;
        OnAllCharactersInside?.Invoke(areAllCharactersInside);
    }
}
