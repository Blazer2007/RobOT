using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Habilidade "braço magnético": mantém o clique para agarrar o MaterialObject magnético
/// que está no centro do ecrã. O objeto levita à frente da mira (à distância a que foi
/// agarrado), movido por velocidade para respeitar as colisões. Ao largar o clique, cai.
/// Configuração em ModuleData: Range (alcance), Force (reatividade), Cooldown, MaxMass.
/// </summary>
 
public class MagneticArm : MonoBehaviour, IRobotAbility, ITargetFeedback
{
    [SerializeField] private LayerMask aimLayers = ~0;
    [SerializeField] private float rayLength = 50f; // tem de cobrir a distância câmara -> alvo
    [SerializeField] private float maxSpeed = 20f;  // velocidade máxima do objeto agarrado

    private readonly RaycastHit[] hits = new RaycastHit[16];
    private readonly List<Collider> userColliders = new List<Collider>();

    private ModuleData data;
    private AbilityContext context;
    private MaterialObject grabbed;
    private Collider[] grabbedColliders;
    private bool originalUseGravity;
    private float holdDistance;
    private float nextUseTime;

    public void Setup(ModuleData moduleData) => data = moduleData;

    public bool CanUse(AbilityContext ctx) =>
        data != null && Time.time >= nextUseTime
        && Evaluate(ctx, out _, out _) == AbilityTargetState.Available;

    // Para a mira do ecrã
    public AbilityTargetState GetTargetState(AbilityContext ctx)
    {
        if (data == null) return AbilityTargetState.None;
        if (grabbed != null) return AbilityTargetState.Active;
        return Evaluate(ctx, out _, out _);
    }

    public void Use(AbilityContext ctx)
    {
        if (grabbed != null || data == null) return;
        if (Evaluate(ctx, out MaterialObject target, out float distance) != AbilityTargetState.Available) return;

        context = ctx;
        grabbed = target;
        holdDistance = distance;

        grabbed.SetGrabbed(true);
        grabbed.ForcedRelease += Release;

        originalUseGravity = grabbed.Body.useGravity;
        grabbed.Body.useGravity = false;

        // Evita subir para a caixa que seguras (a personagem voaria)
        grabbedColliders = grabbed.GetComponentsInChildren<Collider>();
        SetIgnoreUserCollisions(true, ctx.User);
    }

    public void Release()
    {
        if (grabbed == null) return;

        grabbed.ForcedRelease -= Release;
        SetIgnoreUserCollisions(false, null);

        Rigidbody body = grabbed.Body;
        body.useGravity = originalUseGravity;
        body.linearVelocity = Vector3.zero;   // cai diretamente
        body.angularVelocity = Vector3.zero;

        grabbed.SetGrabbed(false);
        grabbed = null;
        grabbedColliders = null;
        context = null;

        nextUseTime = Time.time + (data != null ? data.Cooldown : 0f);
    }

    private void OnDestroy() => Release();

    private void FixedUpdate()
    {
        if (grabbed == null) return;

        Rigidbody body = grabbed.Body;
        Vector3 toTarget = GetHoldPoint() - body.position;

        body.linearVelocity = Vector3.ClampMagnitude(toTarget * data.Force, maxSpeed);
        body.angularVelocity = Vector3.zero;
    }

    // Ponto para onde o objeto vai: na linha da mira, limitado ao alcance a partir da personagem
    private Vector3 GetHoldPoint()
    {
        Vector3 point = context.AimRay.GetPoint(holdDistance);
        Vector3 userPosition = context.User.transform.position;
        Vector3 offset = point - userPosition;

        if (offset.magnitude > data.Range)
            point = userPosition + offset.normalized * data.Range;

        return point;
    }

    // O primeiro objeto atingido pelo raio (ignorando a própria personagem) decide:
    //   não magnético / livre demais / fora do alcance -> None
    //   magnético mas pesado demais                    -> Blocked
    //   magnético, leve e ao alcance                   -> Available
    // Se for uma parede, não agarra (não se agarra através de paredes).
    private AbilityTargetState Evaluate(AbilityContext ctx, out MaterialObject target, out float distance)
    {
        target = null;
        distance = 0f;

        int count = Physics.RaycastNonAlloc(ctx.AimRay, hits, rayLength, aimLayers, QueryTriggerInteraction.Ignore);
        Transform user = ctx.User.transform;

        int best = -1;
        float bestDistance = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            if (hits[i].collider.transform.IsChildOf(user)) continue;
            if (hits[i].distance < bestDistance)
            {
                best = i;
                bestDistance = hits[i].distance;
            }
        }

        if (best < 0) return AbilityTargetState.None;

        RaycastHit hit = hits[best];
        MaterialObject candidate = hit.collider.GetComponentInParent<MaterialObject>();

        if (candidate == null || !candidate.IsMagnetic || candidate.IsGrabbed) return AbilityTargetState.None;
        if (Vector3.Distance(user.position, hit.point) > data.Range) return AbilityTargetState.None;
        if (candidate.Body.mass > data.MaxMass) return AbilityTargetState.Blocked;

        target = candidate;
        distance = hit.distance;
        return AbilityTargetState.Available;
    }

    private void SetIgnoreUserCollisions(bool ignore, GameObject user)
    {
        if (ignore)
        {
            userColliders.Clear();
            user.GetComponentsInChildren(userColliders);
        }

        if (grabbedColliders != null)
        {
            foreach (Collider mine in grabbedColliders)
                foreach (Collider other in userColliders)
                    if (mine != null && other != null)
                        Physics.IgnoreCollision(mine, other, ignore);
        }

        if (!ignore) userColliders.Clear();
    }
}