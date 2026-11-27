using System;
using UnityEngine;

/// <summary>
/// Avalia os requisitos do puzzle e avisa a trapdoor quando ficam cumpridos (ou deixam de estar).
/// </summary>
public class TrapdoorOpener : MonoBehaviour
{
    [SerializeField] private MonoBehaviour[] requirementSources; // Requisitos (IRequirement)
    [SerializeField] private GroupMode mode = GroupMode.All;     // All = todos | Any = pelo menos um

    private RequirementGroup group;

    public event Action<bool> OnPuzzleComplete;

    // Permite à trapdoor perguntar o estado atual (início do jogo e reset)
    public bool IsComplete => group != null && group.IsMet;

    private void Awake()
    {
        group = RequirementGroupFactory.Create(requirementSources, mode, this);
    }

    private void OnEnable()
    {
        group.StartListening();
        group.Changed += OnRequirementsChanged;
    }

    private void OnDisable()
    {
        group.Changed -= OnRequirementsChanged;
        group.StopListening();
    }

    private void OnRequirementsChanged() => OnPuzzleComplete?.Invoke(group.IsMet);
}