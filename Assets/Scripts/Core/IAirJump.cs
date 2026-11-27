// Salto no ar (ex.: propulsor). O PlayerMover só conhece isto: quando a personagem está no ar
// e o jogador carrega em saltar, pergunta se há salto no ar e, se houver, pede-o.
// Quem decide (módulo equipado, cargas, cooldown) é quem implementa (o ModuleController).
public interface IAirJump
{
    bool CanAirJump { get; }
    void AirJump();
}
