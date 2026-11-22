using System;
using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// Objeto físico feito de um ObjectMaterial (caixa de cartão, madeira, ferro...).
/// - A massa vem de densidade x volume dos colliders (Rigidbody.SetDensity), por isso
///   mudar a escala da caixa no editor muda a massa (o volume cresce com o cubo da escala).
/// - O braço magnético só agarra se o material for magnético (IsMagnetic) e leve o suficiente.
/// - Substitui o ResettableTransform nas caixas: se estiver agarrada, solta-a primeiro.
/// </summary>
// Nota: a massa é recalculada no Awake (ao entrar em Play); em edit mode o Inspector
// mostra o valor antigo do Rigidbody.
[RequireComponent(typeof(Rigidbody))]
public class MaterialObject : MonoBehaviour, IResetlable
{
    [SerializeField] private ObjectMaterial material;
    [SerializeField] private float fallLimitY = -20f; // abaixo disto volta ao sítio original

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private MeshRenderer meshRender;
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
        meshRender = GetComponentInChildren<MeshRenderer>();
        if(material.MaterialVisual != null)
            meshRender.material = material.MaterialVisual;

        if (material != null)
            Body.SetDensity(material.Density);
        else
            Debug.LogWarning($"{name}: sem ObjectMaterial, mantém a massa do Rigidbody.", this);
    }
    private void FixedUpdate()
    {
        if (transform.position.y < fallLimitY) ReturnToOrigin();
    }

    public void SetGrabbed(bool grabbed) => IsGrabbed = grabbed;

    public void ResetState() => ReturnToOrigin();

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