using System;
using UnityEngine;

/// <summary>
/// Move a trapdoor: fecha quando as personagens necessárias estão aprisionadas
/// e abre quando o puzzle é completado.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class TrapdoorMover : MonoBehaviour, IResetlable
{
    [SerializeField] private TrapdoorDetector trapdoorDetector; // Filho com o trigger
    [SerializeField] private TrapdoorOpener trapdoorOpener;
    [SerializeField] private Vector3 shutOffset = new Vector3(0, -3, 0);
    [SerializeField][Range(0.5f, 6f)] private float speed = 2f;

    private Rigidbody rb;
    private Vector3 openPosition;
    private Vector3 shutPosition;

    private bool charactersTrapped;   // Jogadores já ficaram aprisionados
    private bool puzzleComplete;      // Requisitos do puzzle cumpridos
    private bool shutNotified;        // Evita emitir OnTrapdoorShut várias vezes

    // Fecha apenas se estão aprisionados E o puzzle ainda não está completo
    private bool ShouldBeShut => charactersTrapped && !puzzleComplete;

    public event Action OnTrapdoorShut;

    private void Awake()
    {
        if (trapdoorDetector == null) trapdoorDetector = GetComponentInChildren<TrapdoorDetector>();
        if (trapdoorOpener == null) trapdoorOpener = GetComponent<TrapdoorOpener>();

        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        openPosition = transform.position;
        shutPosition = openPosition + shutOffset;
    }

    private void OnEnable()
    {
        if (trapdoorOpener != null) trapdoorOpener.OnPuzzleComplete += OnPuzzleChanged;
        if (trapdoorDetector != null) trapdoorDetector.OnRequiredCharactersTrapped += OnTrapped;
    }

    private void OnDisable()
    {
        if (trapdoorOpener != null) trapdoorOpener.OnPuzzleComplete -= OnPuzzleChanged;
        if (trapdoorDetector != null) trapdoorDetector.OnRequiredCharactersTrapped -= OnTrapped;
    }

    // Start corre depois de todos os Awake/OnEnable, por isso o opener já está pronto
    private void Start()
    {
        puzzleComplete = trapdoorOpener != null && trapdoorOpener.IsComplete;
    }

    private void OnTrapped()
    {
        charactersTrapped = true;
    }

    private void OnPuzzleChanged(bool complete)
    {
        puzzleComplete = complete;
        if (complete) charactersTrapped = false; // Ao abrir, limpa o estado de aprisionamento
    }

    private void FixedUpdate()
    {
        Vector3 target = ShouldBeShut ? shutPosition : openPosition;

        if ((rb.position - target).sqrMagnitude < 0.000001f)
        {
            // Chegou ao destino: avisa só uma vez que fechou
            if (ShouldBeShut && !shutNotified)
            {
                shutNotified = true;
                OnTrapdoorShut?.Invoke();
            }
            return;
        }

        shutNotified = false;
        rb.MovePosition(Vector3.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime));
    }

    public void ResetState()
    {
        // Os jogadores voltam ao spawn, por isso já não estão aprisionados
        charactersTrapped = false;
        puzzleComplete = trapdoorOpener != null && trapdoorOpener.IsComplete; // Pergunta aos requisitos, não assume
        shutNotified = false;

        transform.position = openPosition;
        rb.position = openPosition;
        Physics.SyncTransforms();
    }
}