using UnityEngine;

/// <summary>
/// Habilidade "propulsor": um impulso para cima quando se carrega no salto já no ar
/// (duplo salto). Só pode ser usada uma vez por salto: recarrega ao tocar no chão.
/// Configuração em ModuleData: Force (velocidade de subida em m/s) e Cooldown.
/// Altura aproximada = Force² / (2 x 9,81). Ex.: Force 8 -> ~3,3 m.
/// </summary>
public class Thruster : MonoBehaviour, IRobotAbility
{
    private ModuleData data;
    private Rigidbody body;
    private IGroundedState ground;
    private bool used;       // o impulso deste salto já foi gasto
    private float nextUseTime;

    private void Awake() => ResolveReferences();

    public void Setup(ModuleData moduleData) => data = moduleData;

    // Recarrega ao pousar. Corre mesmo quando a personagem não está a ser controlada.
    private void Update()
    {
        if (used && ground != null && ground.IsGrounded) used = false;
    }

    public bool CanUse(AbilityContext context)
    {
        if (data == null) return false;
        ResolveReferences();

        return body != null && ground != null
            && !used
            && !ground.IsGrounded
            && Time.time >= nextUseTime;
    }

    public void Use(AbilityContext context)
    {
        if (!CanUse(context)) return;

        Vector3 velocity = body.linearVelocity;
        velocity.y = data.Force; // define a velocidade vertical, como o salto normal
        body.linearVelocity = velocity;

        used = true;
        nextUseTime = Time.time + data.Cooldown;
    }

    // O propulsor é instantâneo: nada a largar
    public void Release() { }

    // A habilidade é filha da personagem (ModuleController instancia-a lá)
    private void ResolveReferences()
    {
        if (body == null) body = GetComponentInParent<Rigidbody>();
        if (ground == null) ground = GetComponentInParent<IGroundedState>();
    }
}
