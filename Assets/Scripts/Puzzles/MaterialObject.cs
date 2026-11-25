using System;
using UnityEngine;

/// <summary>
/// Objeto físico feito de um ObjectMaterial (caixa de cartão, madeira, ferro...).
/// - Massa = densidade x volume dos colliders (calculada no Awake), por isso mudar a
///   escala da caixa no editor muda a massa (o volume cresce com o cubo da escala).
/// - O braço magnético só agarra se o material for magnético (IsMagnetic) e leve o suficiente.
/// - Se o material não for empurrável (PushableByPlayer = false), o objeto fica com X/Z e a
///   rotação congelados quando não está agarrado: só cai na vertical e o jogador não o move.
/// - Substitui o ResettableTransform nas caixas: se estiver agarrada, solta-a primeiro.
/// </summary>
// Nota: a massa é recalculada no Awake (ao entrar em Play); em edit mode o Inspector
// mostra o valor antigo do Rigidbody.
[RequireComponent(typeof(Rigidbody))]
public class MaterialObject : MonoBehaviour, IResetlable
{
    private const float MinMass = 0.01f;

    [SerializeField] private ObjectMaterial material;
    [SerializeField] private Vector3 fallLimitY = new Vector3(0,-20,0); // abaixo disto volta ao sítio original

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private RigidbodyConstraints originalConstraints;

    public Rigidbody Body { get; private set; }
    public bool IsGrabbed { get; private set; }
    public bool IsMagnetic => material != null && material.IsMagnetic;

    // O braço escuta isto para largar o objeto quando ele é reposto
    public event Action ForcedRelease;

    private void Awake()
    {
        Body = GetComponent<Rigidbody>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        originalConstraints = Body.constraints;


        if (material == null)
        {
            Debug.LogWarning($"{name}: sem ObjectMaterial, mantém a massa do Rigidbody.", this);
            return;
        }

        ApplyVisual();
        ApplyMass();
        ApplyConstraints();
    }

    private void FixedUpdate()
    {
        if (transform.position.y < initialPosition.y + fallLimitY.y) ReturnToOrigin();
    }

    public void SetGrabbed(bool grabbed)
    {
        IsGrabbed = grabbed;
        ApplyConstraints();
    }

    public void ResetState() => ReturnToOrigin();

    private void ApplyVisual()
    {
        if (material.MaterialVisual == null) return;

        // sharedMaterial: não cria uma cópia do material por caixa
        MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null) meshRenderer.sharedMaterial = material.MaterialVisual;
    }

    private void ApplyMass()
    {
        float volume = ComputeVolume();
        if (volume <= 0f)
        {
            Debug.LogWarning($"{name}: sem colliders sólidos, mantém a massa do Rigidbody.", this);
            return;
        }

        Body.mass = Mathf.Max(MinMass, material.Density * volume);
    }

    // Objetos não empurráveis: só caem na vertical enquanto não estão agarrados.
    private void ApplyConstraints()
    {
        if (material == null || material.PushableByPlayer) return;

        Body.constraints = IsGrabbed
            ? originalConstraints
            : RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
    }

    // Volume (m³) de todos os colliders sólidos, com a escala aplicada.
    // Caixa e esfera são exatos; os restantes (cápsula, mesh) usam a caixa envolvente (aproximação).
    private float ComputeVolume()
    {
        float volume = 0f;

        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            if (col.isTrigger) continue;

            Vector3 scale = col.transform.lossyScale;

            switch (col)
            {
                case BoxCollider box:
                    volume += Mathf.Abs(box.size.x * scale.x * box.size.y * scale.y * box.size.z * scale.z);
                    break;

                case SphereCollider sphere:
                    {
                        float radius = sphere.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
                        volume += 4f / 3f * Mathf.PI * radius * radius * radius;
                        break;
                    }

                default:
                    {
                        Vector3 size = col.bounds.size;
                        volume += size.x * size.y * size.z;
                        break;
                    }
            }
        }

        return volume;
    }

    private void ReturnToOrigin()
    {
        ForcedRelease?.Invoke();

        transform.SetPositionAndRotation(initialPosition, initialRotation);
        Body.position = initialPosition;
        Body.rotation = initialRotation;

        if (!Body.isKinematic)
        {
            Body.linearVelocity = Vector3.zero;
            Body.angularVelocity = Vector3.zero;
        }

        Physics.SyncTransforms();
    }
}