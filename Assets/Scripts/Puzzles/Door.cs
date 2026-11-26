using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Door : MonoBehaviour, IResetlable
{
    [SerializeField] private MonoBehaviour[] requirementSources; // Requisitos
    [SerializeField] private GroupMode mode = GroupMode.All; // Modo de verificação dos requisitos 
    [SerializeField] private bool stayOpen; // Se a porta fica aberta depois de abrir ou fecha depois de alguns segundo aberta
    [SerializeField] private Vector3 openOffset = new Vector3(0f, 3f, 0f); // Offset da abertura da porta(a ser mudado)
    [SerializeField] private float speed = 3f;// Velocidade de abertura

    private Rigidbody body; // Rigidbody da porta
    private RequirementGroup group; // Referencia à classe que trata dos grupos de requisitos
    private Vector3 closedPosition; // Posição da porta fechada
    private Vector3 openPosition; // Posição da porta aberta
    private bool shouldBeOpen; // Se a porta deve estar fechada ou não
    public bool IsOpen => shouldBeOpen; // A condição da porta estar aberta depende da condição da porta poder estar aberta
    // Esta função pega no valor de "shouldBeOpen" e atribui-o a "IsOpen"

    
    public event Action<bool> OpenStateChanged; // Indicação da mudança de estado da porta(aberta/fechada)

    private void Awake()
    {
        body = GetComponent<Rigidbody>(); // Pega no Rigidbody da porta
        body.isKinematic = true; // Garante que é kinematic(forças não afetam a porta, só se mexe com código)
        body.interpolation = RigidbodyInterpolation.Interpolate; // garante que faz interpolate(suaviza o movimento com
                                                                 // cálculos de posições anteriores

        closedPosition = transform.position; // Armazena o valor da posição da porta fechada como
                                             // o valor da posição inicial da mesma

        openPosition = closedPosition + transform.rotation * openOffset; // Armazena a posição da porta aberta como
                                                                         // a soma da posição da porta fechada + o offset da porta 

        group = RequirementGroupFactory.Create(requirementSources, mode, this);
    }

    private void OnEnable() // Enquanto o objeto está ativo
    {
        group.StartListening(); // Fica à escuta de mudanças nos requisitos
        group.Changed += OnRequirementsChanged; // Subscrever-se às mudanças dos requisitos

        shouldBeOpen = group.IsMet; // A porta deve estar aberta se todos os requisitos forem cumpridos
        transform.position = shouldBeOpen ? openPosition : closedPosition; // Altera a posição da porta de acordo
                                                                           // com a condição shouldBeOpen
    }

    private void OnDisable() // Enquanto o objeto está desativo
    {
        group.Changed -= OnRequirementsChanged; // Dessubscrever-se das mudanças dos requisitos
        group.StopListening(); // Deixar de ouvir mudanças
    }

    // Se houver mudanças nos requisitos
    private void OnRequirementsChanged()
    {
        if (stayOpen && shouldBeOpen) return; // se a porta ficar aberta e já dever estar aberta não faz nada
        SetOpen(group.IsMet); // Se todos os requitos forem cumpridos abre-se e a porta, caso contrário não

        // Se a porta já estiver aberta e um dos requisitos voltar a desativar a porta fecha
    }
    public void ResetState()
    {
        shouldBeOpen = group.IsMet;   // volta a perguntar aos requisitos, não assume nada
        Vector3 position = shouldBeOpen ? openPosition : closedPosition; // Guarda a posição da porta, fazendo a mesma
                                                                         // verificação de abertura

        transform.position = position; // a posição da porta passa a ser a posição anteriormente guardada
        body.position = position; // o mesmo para o rigidbody da porta
        Physics.SyncTransforms(); // ajustar os transforms da porta e do seu rigidbody
    }

    // Abrir a porta dependendo da verificação do parametro
    private void SetOpen(bool open)
    {
        if (open == shouldBeOpen) return; // Se o parametro passado for igual ao shouldBeOpen não faz nada
        shouldBeOpen = open; // caso contrário o valor do shouldBeOpen passa a ser o valor do parametro
        OpenStateChanged?.Invoke(open); // Indica a mudança da abertura da porta
    }

    private void FixedUpdate()
    {
        Vector3 target = shouldBeOpen ? openPosition : closedPosition;

        if ((body.position - target).sqrMagnitude < 0.000001f) return; // Deixa de mexer a porta se chegar ao destino

        // Move o rigidbody até à posição destino
        body.MovePosition(Vector3.MoveTowards(body.position, target, speed * Time.fixedDeltaTime));
    }

    // Mostra o percurso da porta
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green; // cor da linha a desenhar
        Vector3 end = transform.position + transform.rotation * openOffset; // Posição final do precurso
        Gizmos.DrawLine(transform.position, end); // Desenhar linha entre o inicio e o fim
        Gizmos.DrawWireSphere(end, 0.15f); // Desenhar esfera no fim *ESTETICS*
    }
}