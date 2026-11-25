using UnityEngine;

// Sons do O.T:
// - "acordar": toca uma vez quando o O.T é ligado no terminal (evento PoweredOn do RobotPowerUp)
// - "andar": em loop enquanto o O.T está ligado, no chão e a mexer-se (seja o jogador ou a IA a controlá-lo)
// Cria as AudioSources sozinho, por isso no prefab só é preciso pôr os clips.
[RequireComponent(typeof(RobotPowerUp), typeof(Rigidbody))]
public class OTSounds : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip wakeUpClip;
    [SerializeField] private AudioClip walkClip;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float wakeUpVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float walkVolume = 0.6f;
    [Tooltip("Tempo (s) para o som de andar aparecer/desaparecer, para não dar estalos.")]
    [SerializeField] private float walkFadeTime = 0.15f;

    [Header("Andar")]
    [Tooltip("Velocidade horizontal mínima (m/s) para contar como a andar.")]
    [SerializeField] private float minWalkSpeed = 0.3f;

    [Header("3D")]
    [Tooltip("0 = som 2D (ouve-se igual em todo o lado), 1 = som 3D (depende da distância).")]
    [SerializeField, Range(0f, 1f)] private float spatialBlend = 1f;
    [SerializeField] private float maxDistance = 25f;

    private RobotPowerUp powerUp;
    private Rigidbody body;
    private PlayerMover mover;
    private AudioSource oneShotSource;
    private AudioSource walkSource;

    private void Awake()
    {
        powerUp = GetComponent<RobotPowerUp>();
        body = GetComponent<Rigidbody>();
        mover = GetComponent<PlayerMover>();

        oneShotSource = CreateSource(false);
        walkSource = CreateSource(true);
        walkSource.clip = walkClip;
        walkSource.volume = 0f;
    }

    private void OnEnable() => powerUp.PoweredOn += PlayWakeUp;
    private void OnDisable() => powerUp.PoweredOn -= PlayWakeUp;

    private void PlayWakeUp()
    {
        if (wakeUpClip != null) oneShotSource.PlayOneShot(wakeUpClip, wakeUpVolume);
    }

    private void Update()
    {
        if (walkClip == null) return;

        float target = IsWalking() ? walkVolume : 0f;
        float step = walkFadeTime > 0f ? walkVolume / walkFadeTime * Time.deltaTime : 1f;
        walkSource.volume = Mathf.MoveTowards(walkSource.volume, target, step);

        // Só fica a tocar enquanto se ouve; quando o volume chega a 0 pára
        if (walkSource.volume > 0f && !walkSource.isPlaying) walkSource.Play();
        else if (walkSource.volume <= 0f && walkSource.isPlaying) walkSource.Stop();
    }

    private bool IsWalking()
    {
        if (!powerUp.IsPowered) return false;
        if (mover != null && !mover.IsGrounded) return false; // no ar não há passos

        Vector3 v = body.linearVelocity;
        v.y = 0f;
        return v.sqrMagnitude > minWalkSpeed * minWalkSpeed;
    }

    private AudioSource CreateSource(bool loop)
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = spatialBlend;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 1f;
        source.maxDistance = maxDistance;
        return source;
    }
}
