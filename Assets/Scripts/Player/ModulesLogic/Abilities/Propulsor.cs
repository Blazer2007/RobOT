using UnityEngine;
/// <summary>
/// Habilidade "propulsor" (módulo do slot EXTRA, ativação AirJump):
/// no ar, carregar outra vez em saltar dá um impulso para cima (duplo salto).
/// As cargas voltam quando a personagem toca no chão.
/// Configuração em ModuleData: Force = altura do impulso (m), Cooldown = tempo entre impulsos (s).
/// Quem chama Use() é o ModuleController, a pedido do PlayerMover (IAirJump).
/// </summary>
public class Propulsor : MonoBehaviour, IRobotAbility
{
    [SerializeField] private int airJumps = 1;            // impulsos por cada vez que está no ar
    [SerializeField] private AudioClip boostSound;        // opcional
    [SerializeField, Range(0f, 1f)] private float boostVolume = 0.8f;
    [SerializeField] private ParticleSystem boostEffect;  // opcional (filho deste prefab)

    private ModuleData data;
    private Rigidbody body;
    private PlayerMover mover;
    private int used;
    private float nextUseTime;

    public void Setup(ModuleData moduleData) => data = moduleData;

    public bool CanUse(AbilityContext context)
    {
        if (data == null || !Cache(context)) return false;
        return !mover.IsGrounded && used < airJumps && Time.time >= nextUseTime;
    }

    public void Use(AbilityContext context)
    {
        if (!CanUse(context)) return;

        // Mesmo cálculo do salto normal: velocidade para subir "Force" metros
        float height = Mathf.Max(0f, data.Force);
        Vector3 velocity = body.linearVelocity;
        velocity.y = Mathf.Sqrt(2f * -Physics.gravity.y * height);
        body.linearVelocity = velocity;

        used++;
        nextUseTime = Time.time + data.Cooldown;

        if (boostSound != null) AudioSource.PlayClipAtPoint(boostSound, body.position, boostVolume);
        if (boostEffect != null) boostEffect.Play();
    }

    public void Release() { } // não fica "mantido" como o braço: é um impulso só

    private void FixedUpdate()
    {
        if (mover != null && mover.IsGrounded) used = 0; // tocou no chão: recarrega
    }

    private bool Cache(AbilityContext context)
    {
        if (body != null && mover != null) return true;
        if (context?.User == null) return false;
        body = context.User.GetComponent<Rigidbody>();
        mover = context.User.GetComponent<PlayerMover>();
        return body != null && mover != null;
    }
}
