using System;
using System.Collections.Generic;
using UnityEngine;

public class CheckPlayerCount : MonoBehaviour
{
    [SerializeField] private int totalPlayers = 2;
    private List<GameObject> playersInside = new List<GameObject>();
    [HideInInspector] public bool areAllCharactersInside = false;
    public event Action<bool> AllCharactersInside;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Rob") || other.CompareTag("OT"))
        {
            if (!playersInside.Contains(other.gameObject))
            {
                playersInside.Add(other.gameObject);
            }

            CheckPlayers();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Rob") || other.CompareTag("OT"))
        {
            playersInside.Remove(other.gameObject);

            CheckPlayers();
        }
    }

    private void CheckPlayers()
    {
        areAllCharactersInside = playersInside.Count >= totalPlayers;
        AllCharactersInside?.Invoke(areAllCharactersInside);
    }
}
