using System;
using System.Collections;
using UnityEngine;

public class ElevatorMover : MonoBehaviour, IInteractable
{
    [SerializeField] private Vector3 moveOffset = new Vector3(0f, -3f, 0f);
    [SerializeField][Range(1f,3f)] private float speed = 3f;
    [SerializeField] private string prompt = "Go to next level";
    private Rigidbody rb;
    private Animator anim;
    private Transform parentTransform;
    public string Prompt => prompt;
    public event Action ElevatorIsGoingDown; 
    private bool IsActive = false;
    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
        anim = GetComponentInParent<Animator>();
        parentTransform = rb.gameObject.GetComponent<Transform>();
    }
    private void FixedUpdate()
    {
        if (IsActive)
            MoveElevator();
    }
    public void MoveElevator()
    {
        StartCoroutine(MovingCorroutine());
        ElevatorIsGoingDown?.Invoke();
    }
    private IEnumerator MovingCorroutine() 
    {
        yield return new WaitForSeconds(2f);
        Vector3 current = rb.position;
        Vector3 next = Vector3.MoveTowards(current, current+moveOffset, speed * Time.fixedDeltaTime);

        parentTransform.position = rb.position;
        rb.MovePosition(next);
        Physics.SyncTransforms();
        
        yield return new WaitForSeconds(4f);
    }
    public bool CanInteract(GameObject Interactor) => !IsActive;

    public void Interact(GameObject interactor) 
    {
        if (!CanInteract(interactor)) return;
        anim.SetBool("IsOpen", false);
        IsActive = true;
    }
}
