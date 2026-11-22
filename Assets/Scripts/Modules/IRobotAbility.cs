// IRobotAbility.cs
using UnityEngine;

// Contexto que o ModuleController entrega às habilidades.
// AimRay é calculado sempre que é lido, por isso uma habilidade que fique ativa
// (ex.: braço a arrastar uma caixa) tem sempre a mira atualizada (centro do ecrã).
public class AbilityContext
{
    public GameObject User { get; }
    public Camera Camera { get; }

    public AbilityContext(GameObject user, Camera camera)
    {
        User = user;
        Camera = camera;
    }

    public Ray AimRay => Camera != null
        ? Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f))
        : new Ray(User.transform.position, User.transform.forward);
}

// Contrato de uma capacidade do robô. O ModuleController só conhece isto:
// acrescentar o gancho é criar uma classe nova que o implementa (OCP).
public interface IRobotAbility
{
    void Setup(ModuleData data);          // recebe a configuração (alcance, força, cooldown)
    bool CanUse(AbilityContext context);  // pode ser usada agora? (cooldown, alvo válido...)
    void Use(AbilityContext context);     // clique premido: começa
    void Release();                       // clique largado, ou o módulo foi largado/trocado
}
