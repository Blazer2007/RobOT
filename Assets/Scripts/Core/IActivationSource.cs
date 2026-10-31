using System;
public interface IActivationSource
{
    bool IsActive { get; }
    event Action<bool> ActivationChanged;   // true = ativou, false = desativou
}