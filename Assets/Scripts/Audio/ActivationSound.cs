using UnityEngine;

// Toca um som quando uma fonte de ativação (IActivationSource) liga ou desliga.
// Serve para qualquer mecanismo sem mexer no script dele: placa de pressão (pressionar / largar),
// alavanca (puxar), terminal, elevador...
// Cria a AudioSource sozinho, por isso no prefab só é preciso pôr os clips.
public class ActivationSound : MonoBehaviour
{
    [Tooltip("Vazio = o IActivationSource deste mesmo objeto.")]
    [SerializeField] private MonoBehaviour sourceBehaviour;
    [SerializeField] private AudioClip activatedClip;   // ao ligar (ex.: placa pressionada, alavanca puxada)
    [SerializeField] private AudioClip deactivatedClip; // ao desligar (vazio = sem som)
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    [Header("3D")]
    [Tooltip("0 = som 2D (igual em todo o lado), 1 = som 3D (depende da distância).")]
    [SerializeField, Range(0f, 1f)] private float spatialBlend = 1f;
    [SerializeField] private float maxDistance = 20f;

    private IActivationSource source;
    private AudioSource audioSource;

    private void Awake()
    {
        source = sourceBehaviour != null ? sourceBehaviour as IActivationSource : GetComponent<IActivationSource>();
        if (source == null)
            Debug.LogError($"{name}: ActivationSound sem IActivationSource.", this);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = spatialBlend;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = maxDistance;
    }

    private void OnEnable()
    {
        if (source != null) source.ActivationChanged += Play;
    }

    private void OnDisable()
    {
        if (source != null) source.ActivationChanged -= Play;
    }

    private void Play(bool active)
    {
        AudioClip clip = active ? activatedClip : deactivatedClip;
        if (clip != null) audioSource.PlayOneShot(clip, volume);
    }
}
