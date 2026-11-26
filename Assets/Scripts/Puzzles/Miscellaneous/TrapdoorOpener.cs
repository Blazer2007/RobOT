using System;
using UnityEngine;

/// <summary>
/// Abre a trapdoor quando os requisitos do puzzle são cumpridos
/// </summary>
public class TrapdoorOpener : MonoBehaviour
{
    [SerializeField] private MonoBehaviour[] requirementSources; // Requisitos
    [SerializeField] private GroupMode mode = GroupMode.All; // Modo de verificação dos requisitos 

    private RequirementGroup group; // Referencia à classe que trata dos grupos de requisitos
    public event Action<bool> OnPuzzleComplete;

    private void Awake()
    {
        group = RequirementGroupFactory.Create(requirementSources, mode, this);
    }

    private void OnEnable() // Enquanto o objeto está ativo
    {
        group.StartListening(); // Fica à escuta de mudanças nos requisitos
        group.Changed += OnRequirementsChanged; // Subscrever-se às mudanças dos requisitos
    }
    private void OnDisable()
    {
        group.Changed -= OnRequirementsChanged;
        group.StopListening();
    }

    private void OnRequirementsChanged()
    {
        SetOpen(group.IsMet); // Se todos os requitos forem cumpridos abre-se e a porta, caso contrário não
    }
    private void SetOpen(bool open)
    {
        OnPuzzleComplete?.Invoke(open); // Indica que a trapdoor deve abrir quando todos os requisitos forem cumpridos
    }
}
