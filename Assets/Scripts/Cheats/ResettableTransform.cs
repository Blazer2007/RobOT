using UnityEngine;

public class ResettableTransform : MonoBehaviour, IResetlable
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private Rigidbody rb;
    private CharacterController cc;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();
    }

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    public void ResetState()
    {
        gameObject.SetActive(true);

        MoveTo(initialPosition, initialRotation);
    }


    public void MoveTo(Vector3 ps, Quaternion rt)
    {
        if(cc != null) cc.enabled = false;

        transform.SetLocalPositionAndRotation(ps, rt);

        if(cc != null) cc.enabled = true;

        if(rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
