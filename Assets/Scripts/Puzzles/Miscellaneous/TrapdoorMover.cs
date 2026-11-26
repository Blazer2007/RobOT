using System;
using UnityEngine;

/// <summary>
/// Fecha/Abre a porta dependendo do cumprimento dos requisitos referidos no TrapdoorDetector
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class TrapdoorMover : MonoBehaviour, IResetlable
{
    [SerializeField] private TrapdoorDetector trapdoorDetector;
    //[SerializeField] private TrapdoorOpener trapdoorOpener;
    [SerializeField] private Vector3 shutOffset = new Vector3(0,-3,0);
    [SerializeField][Range(0.5f, 6f)] private float speed = 2f;
    private Rigidbody rb;
    private Vector3 openPosition;
    private Vector3 shutPosition;
    private bool shouldBeShut;
    private bool allCharactersInside = false;
    public event Action OnTrapdoorShut;

    private void Awake()
    {
        if (trapdoorDetector == null)
            trapdoorDetector = GetComponent<TrapdoorDetector>();

        rb = GetComponent<Rigidbody>();

        openPosition = transform.position;
        shutPosition = openPosition + shutOffset;
    }

    private void OnEnable()
    {
        //trapdoorOpener.OnPuzzleComplete += OpenTrapDoor;
        trapdoorDetector.OnRequiredCharactersTrapped += ShutTrapDoor;
    }
    private void OnDisable()
    {
        //trapdoorOpener.OnPuzzleComplete -= OpenTrapDoor;
        trapdoorDetector.OnRequiredCharactersTrapped -= ShutTrapDoor;
    }
    /// <summary>
    /// Fecha a trapdoor quando todos os jogadores estão dentro da área respetiva
    /// </summary>
    private void ShutTrapDoor() 
    {
        allCharactersInside = true;
        if (transform.position == shutPosition)
            OnTrapdoorShut?.Invoke();
    }
    //private void OpenTrapDoor()
    //{
    //    allCharactersInside = true;
    //    if (transform.position == shutPosition)
    //        OnTrapdoorShut?.Invoke();
    //}
    private void FixedUpdate()
    {
        Vector3 target = shouldBeShut ? shutPosition : openPosition;

        if ((rb.position - target).sqrMagnitude < 0.000001f) return;

        rb.MovePosition(Vector3.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime));
    }
    public void ResetState()
    {
        shouldBeShut = allCharactersInside;
        Vector3 position = shouldBeShut ? shutPosition : openPosition; 

        transform.position = position; 
        rb.position = position; 
        Physics.SyncTransforms();
    }
}
