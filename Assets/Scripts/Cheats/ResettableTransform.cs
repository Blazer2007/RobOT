using UnityEngine;

public class ResettableTransform : MonoBehaviour, IResetlable
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private Rigidbody rb;
    private CharacterController cc;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();
    }

    private void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    public void ResetState()
    {
        gameObject.SetActive(true);
        MoveTo(initialPosition, initialRotation);
    }

    public void MoveTo(Vector3 position, Quaternion rotation)
    {
        if (cc != null) cc.enabled = false;

        transform.SetPositionAndRotation(position, rotation);   // mundo, tal como foi guardado

        if (cc != null) cc.enabled = true;

        if (rb != null)
        {
            rb.position = position;     // atualiza já o corpo físico
            rb.rotation = rotation;

            if (!rb.isKinematic)        // velocidade só existe em corpos dinâmicos
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        Physics.SyncTransforms();       // a física passa a ver já a nova posição
    }
}