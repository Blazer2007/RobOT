using System;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleResetter : MonoBehaviour
{
    [SerializeField] private KeyCode resetKey = KeyCode.R;

    private readonly List<IResetlable> targets = new List<IResetlable>();

    public event Action PuzzleResetPerformed;

    private void Awake()
    {
        foreach (var tag in GetComponentsInChildren<PuzzleResetTarget>(true))
            foreach (var resettable in tag.GetComponents<IResetlable>())
                targets.Add(resettable);

        if (targets.Count == 0)
            Debug.LogWarning($"{name}: nenhum PuzzleResetTarget com IResetlable encontrado.", this);
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;   // não reinicia com o jogo em pausa
        if (Input.GetKeyDown(resetKey)) ResetPuzzle();
    }

    public void ResetPuzzle()
    {
        foreach (var target in targets)
            target.ResetState();

        Physics.SyncTransforms();
        PuzzleResetPerformed?.Invoke();
    }
}