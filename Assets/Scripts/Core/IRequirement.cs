using System;

public interface IRequirement
{
    bool IsMet { get; }
    event Action Changed;   // disparado quando IsMet pode ter mudado
}