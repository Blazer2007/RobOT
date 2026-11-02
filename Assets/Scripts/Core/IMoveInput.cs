using UnityEngine;

public interface IMoveInput
{
    Vector2 Move { get; }       // x = lateral, y = frente/trás (-1 a 1)
    bool ConsumeJump();         // true uma única vez por pedido de salto
}
