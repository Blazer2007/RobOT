using UnityEngine;

public interface IInteractable
{
        string Prompt { get; }// texto mostrado na UI, por exemplo "E - Ativar O.T"
        bool CanInteract(GameObject interactor);// há condições para usar?(por exemplo se uma placa de pressão está ativa para se poder abrir uma porta)
        void Interact(GameObject interactor); // executa a interação (Guarda o objeto que interagiu
                                              // , pode ser importante saber se foi o Rob ou o OT
}
