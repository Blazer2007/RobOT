using UnityEngine;

// Som de andar em loop (passos do Rob, motor do O.T...). Toca enquanto a personagem está no chão
// e a mexer-se, seja o jogador ou a IA a controlá-la. Para não soar repetitivo:
// - pitch aleatório a cada volta do loop
// - pitch acompanha a velocidade (mais rápido = mais agudo)
// Se o objeto tiver RobotPowerUp (O.T), só toca depois de ele ser ligado.
// Cria a AudioSource sozinho, por isso no prefab só é preciso pôr o clip.
[RequireComponent(typeof(Rigidbody))]
public class WalkingLoopSound : MonoBehaviour
{
    [SerializeField] private AudioClip clip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.6f;
    [Tooltip("Tempo (s) para o som aparecer/desaparecer, para não dar estalos.")]
    [SerializeField] private float fadeTime = 0.15f;

    [Header("Andar")]
    [Tooltip("Velocidade horizontal mínima (m/s) para contar como a andar.")]
    [SerializeField] private float minSpeed = 0.3f;
    [Tooltip("Pitch aleatório escolhido a cada volta do loop.")]
    [SerializeField] private Vector2 pitchRange = new Vector2(0.92f, 1.08f);
    [Tooltip("Velocidade (m/s) a que o pitch fica 'normal'.")]
    [SerializeField] private float referenceSpeed = 4f;
    [Tooltip("Quanto a velocidade mexe no pitch (0 = nada).")]
    [SerializeField, Range(0f, 0.5f)] private float speedPitchAmount = 0.15f;

    [Header("3D")]
    [Tooltip("0 = som 2D (igual em todo o lado), 1 = som 3D (depende da distância).")]
    [SerializeField, Range(0f, 1f)] private float spatialBlend = 1f;
    [SerializeField] private float maxDistance = 25f;

    private Rigidbody body;
    private PlayerMover mover;       // opcional: para saber se está no chão
    private RobotPowerUp powerUp;    // opcional: O.T só faz barulho depois de ligado
    private AudioSource source;
    private float randomPitch = 1f;
    private float lastTime;          // para saber quando o loop recomeçou

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        mover = GetComponent<PlayerMover>();
        powerUp = GetComponent<RobotPowerUp>();

        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.clip = clip;
        source.volume = 0f;
        source.spatialBlend = spatialBlend;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 1f;
        source.maxDistance = maxDistance;
    }

    private void Update()
    {
        if (clip == null) return;

        float target = IsWalking() ? volume : 0f;
        float step = fadeTime > 0f ? volume / fadeTime * Time.deltaTime : 1f;
        source.volume = Mathf.MoveTowards(source.volume, target, step);

        // Só fica a tocar enquanto se ouve
        if (source.volume > 0f && !source.isPlaying)
        {
            PickRandomPitch();
            source.Play();
        }
        else if (source.volume <= 0f && source.isPlaying) source.Stop();

        if (!source.isPlaying) return;

        // O tempo do clip "voltou atrás" = o loop recomeçou -> pitch novo
        if (source.time < lastTime) PickRandomPitch();
        lastTime = source.time;

        float speed01 = referenceSpeed > 0f ? HorizontalSpeed() / referenceSpeed : 1f;
        source.pitch = randomPitch * (1f + (Mathf.Clamp(speed01, 0f, 1.5f) - 1f) * speedPitchAmount);
    }

    private void PickRandomPitch()
    {
        randomPitch = Random.Range(pitchRange.x, pitchRange.y);
        lastTime = 0f;
    }

    private bool IsWalking()
    {
        if (powerUp != null && !powerUp.IsPowered) return false;
        if (mover != null && !mover.IsGrounded) return false; // no ar não há passos
        return HorizontalSpeed() > minSpeed;
    }

    private float HorizontalSpeed()
    {
        Vector3 v = body.linearVelocity;
        v.y = 0f;
        return v.magnitude;
    }
}
