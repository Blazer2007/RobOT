using System;

// Personagem que o jogador pode controlar (Rob ou O.T)
public interface IControllable
{
    bool IsControlled { get; }
    event Action<bool> ControlChanged; // true = ganhou o controlo, false = perdeu
    void GainControl();
    void LoseControl();
}
