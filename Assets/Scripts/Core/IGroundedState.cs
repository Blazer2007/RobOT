/// <summary>
/// Contrato mínimo: "esta personagem está a pisar o chão?".
/// Implementado pelo PlayerMover; usado por habilidades (ex.: propulsor) para saberem
/// se estão no ar, sem dependerem da classe concreta do movimento.
/// </summary>
public interface IGroundedState
{
    bool IsGrounded { get; }
}
