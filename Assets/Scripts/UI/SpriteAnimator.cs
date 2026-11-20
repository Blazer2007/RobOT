using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpriteAnimator : MonoBehaviour
{
    public Image targetImage;
    public Sprite[] frames;
    public float frameRate = 10f;
    public bool loop = true;

    private int currentFrame = 0;
    private Coroutine animCoroutine;

    void OnEnable()
    {
        if (frames == null || frames.Length == 0 || targetImage == null)
            return;

        currentFrame = 0;
        animCoroutine = StartCoroutine(PlayAnimation());
    }

    void OnDisable()
    {
        if (animCoroutine != null)
        {
            StopCoroutine(animCoroutine);
            animCoroutine = null;
        }
    }

    IEnumerator PlayAnimation()
    {
        float delay = 1f / frameRate;

        while (true)
        {
            targetImage.sprite = frames[currentFrame];
            currentFrame++;

            if (currentFrame >= frames.Length)
            {
                if (!loop) yield break;
                currentFrame = 0;
            }

            
            yield return new WaitForSecondsRealtime(delay);
        }
    }
}