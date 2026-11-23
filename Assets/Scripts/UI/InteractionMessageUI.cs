using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Mostra o texto da interação (ex.: "E - Ativar O.T") da personagem que estiver a ser controlada.
// Ouve TODOS os PlayerInteractor da cena (Rob e O.T): só o da personagem controlada está ligado,
// por isso o texto segue sempre quem estás a jogar.
public class InteractionMessageUI : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    private readonly List<(PlayerInteractor interactor, Action<IInteractable> handler)> subscriptions =
        new List<(PlayerInteractor, Action<IInteractable>)>();
    private PlayerInteractor owner; // quem está a mostrar o texto agora
    private IInteractable current;  // alvo cujo texto está no ecrã

    private void OnEnable()
    {
        var interactors = FindObjectsByType<PlayerInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var interactor in interactors)
        {
            var source = interactor;
            Action<IInteractable> handler = target => OnFocusChanged(source, target);
            interactor.FocusChanged += handler;
            subscriptions.Add((interactor, handler));
        }
        Show(null);
    }

    private void OnDisable()
    {
        foreach (var (interactor, handler) in subscriptions)
            if (interactor != null) interactor.FocusChanged -= handler;
        subscriptions.Clear();
        owner = null;
        current = null;
    }

    private void OnFocusChanged(PlayerInteractor source, IInteractable target)
    {
        if (target != null) owner = source;
        else if (source != owner) return; // outro interactor a perder o foco não apaga o texto de quem está a jogar
        else owner = null;
        Show(target);
    }

    private void Show(IInteractable target)
    {
        current = target;
        label.gameObject.SetActive(target != null);
        if (target != null) label.text = target.Prompt;
    }

    // O texto pode mudar sem mudar o alvo (ex.: alavanca "Ligar" -> "Desligar")
    private void LateUpdate()
    {
        if (current != null) label.text = current.Prompt;
    }
}
