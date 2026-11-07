using System;
using System.Collections.Generic;
using UnityEngine;

public class ResetLevelManager : MonoBehaviour
{
    [SerializeField] private ResettableTransform rob;
    [SerializeField] private ResettableTransform ot;

    [SerializeField] private Transform spawnRob;
    [SerializeField] private Transform spawnOt;

    public event Action OnLevelReset;

    private List<GameObject> CreatedInGame = new List<GameObject>();

    private IResetlable[] resetlables;

    void Start()
    {
        resetlables = GetComponentsInChildren<IResetlable>();
    }

   public void RegisterSpawned(GameObject obj)
    {
        CreatedInGame.Add(obj);
    }

    public void RestartLevel()
    {
        foreach (GameObject obj in CreatedInGame)
        {
            if (obj != null) Destroy(obj);
        }
        CreatedInGame.Clear();

        foreach(IResetlable r in resetlables)
        {
            if (r as UnityEngine.Object != null) continue;

                r.ResetState();
        }

        rob.transform.SetLocalPositionAndRotation(spawnRob.position, spawnRob.rotation);
        ot.transform.SetLocalPositionAndRotation(spawnOt.position, spawnOt.rotation);

        OnLevelReset?.Invoke();

    }

   
}
