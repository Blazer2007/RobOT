using UnityEngine;

// Som do O.T a acordar: toca uma vez quando o O.T é ligado no terminal (evento PoweredOn do RobotPowerUp).
// O som de andar está no WalkingLoopSound (o mesmo componente dos passos do Rob).
// Cria a AudioSource sozinho, por isso no prefab só é preciso pôr o clip.
[RequireComponent(typeof(RobotPowerUp))]
public class OTSounds : MonoBehaviour
{
    [SerializeField] private AudioClip wakeUpClip;
    [SerializeField, Range(0f, 1f)] private float wakeUpVolume = 1f;
    [Tooltip("O O.T fica parado enquanto o som de acordar toca (assim o som de andar não fica por cima).")]
    [SerializeField] private bool freezeWhileWakingUp = true;

    [Header("3D")]
    [Tooltip("0 = som 2D (igual em todo o lado), 1 = som 3D (depende da distância).")]
    [SerializeField, Range(0f, 1f)] private float spatialBlend = 1f;
    [SerializeField] private float maxDistance = 25f;

    private RobotPowerUp powerUp;
    private AudioSource source;

    private void Awake()
    {
        powerUp = GetComponent<RobotPowerUp>();

        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = spatialBlend;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 1f;
        source.maxDistance = maxDistance;
    }

    private void OnEnable() => powerUp.PoweredOn += PlayWakeUp;
    private void OnDisable()
    {
        powerUp.PoweredOn -= PlayWakeUp;
        Unfreeze(); // se for desligado a meio do som, o O.T não fica preso
    }

    private PlayerMover frozenMover;

    private void Unfreeze()
    {
        if (frozenMover != null) frozenMover.enabled = true;
        frozenMover = null;
    }

    private void PlayWakeUp()
    {
        if (wakeUpClip == null) return;
        source.PlayOneShot(wakeUpClip, wakeUpVolume);
        if (freezeWhileWakingUp) StartCoroutine(FreezeWhileWakingUp(wakeUpClip.length));
    }

    // Desliga o PlayerMover durante o som: o O.T não anda (nem pelo jogador nem a seguir o Rob),
    // por isso o WalkingLoopSound também não toca. A gravidade continua a funcionar.
    private System.Collections.IEnumerator FreezeWhileWakingUp(float duration)
    {
        if (!TryGetComponent(out PlayerMover mover) || !mover.enabled) yield break;

        mover.enabled = false;
        frozenMover = mover;
        if (TryGetComponent(out Rigidbody body))
            body.linearVelocity = new Vector3(0f, body.linearVelocity.y, 0f);

        yield return new WaitForSeconds(duration);

        Unfreeze();
    }
}
