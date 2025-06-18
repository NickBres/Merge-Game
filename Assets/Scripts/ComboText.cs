using UnityEngine;
using System.Collections;

public class ComboText : MonoBehaviour
{
    public static ComboText instance;
    [SerializeField] private GameObject wowText;
    [SerializeField] private GameObject epicText;
    [SerializeField] private GameObject cawabungaText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        wowText.SetActive(false);
        epicText.SetActive(false);
        cawabungaText.SetActive(false);
    }



    public void ShowWOW()
    {
        wowText.SetActive(true);
        FadeIn(wowText, 0.2f);
        ScaleBounce(wowText);
        StartCoroutine(HideAfterDelay(wowText, 1f));
        AudioManager.instance.PlayWowSound();
    }

    public void ShowEpic()
    {
        epicText.SetActive(true);
        FadeIn(epicText, 0.2f);
        Rotate360(epicText);
        ScaleBounce(epicText);
        StartCoroutine(HideAfterDelay(epicText, 1.2f));
        AudioManager.instance.PlayEpicSound();
    }

    public void ShowCawabunga()
    {
        cawabungaText.SetActive(true);
        FadeIn(cawabungaText, 0.2f);
        Shake(cawabungaText);
        ScaleBounce(cawabungaText);
        StartCoroutine(HideAfterDelay(cawabungaText, 1.5f));
        AudioManager.instance.PlayCawabungaSound();
    }

    public void ScaleBounce(GameObject target, float scaleAmount = 1.2f, float duration = 0.2f)
    {
        StartCoroutine(ScaleBounceCoroutine(target.transform, scaleAmount, duration));
    }

    private IEnumerator ScaleBounceCoroutine(Transform target, float scaleAmount, float duration)
    {
        Vector3 originalScale = target.localScale;
        Vector3 targetScale = originalScale * scaleAmount;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float factor = t / duration;
            target.localScale = Vector3.Lerp(originalScale, targetScale, factor);
            yield return null;
        }

        t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float factor = t / duration;
            target.localScale = Vector3.Lerp(targetScale, originalScale, factor);
            yield return null;
        }
    }

    public void Rotate360(GameObject target, float duration = 0.5f)
    {
        StartCoroutine(RotateCoroutine(target.transform, duration));
    }

    private IEnumerator RotateCoroutine(Transform target, float duration)
    {
        float elapsed = 0f;
        Quaternion originalRotation = target.rotation;
        while (elapsed < duration)
        {
            float angle = Mathf.Lerp(0f, 360f, elapsed / duration);
            target.rotation = originalRotation * Quaternion.Euler(0, 0, angle);
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.rotation = originalRotation;
    }

    public void Shake(GameObject target, float magnitude = 5f, float duration = 0.2f)
    {
        StartCoroutine(ShakeCoroutine(target.transform, magnitude, duration));
    }

    private IEnumerator ShakeCoroutine(Transform target, float magnitude, float duration)
    {
        Vector3 originalPos = target.localPosition;
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            target.localPosition = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.localPosition = originalPos;
    }

    public void FadeOut(GameObject target, float duration = 1.0f)
    {
        StartCoroutine(FadeCoroutine(target, 1f, 0f, duration));
    }

    public void FadeIn(GameObject target, float duration = 1.0f)
    {
        StartCoroutine(FadeCoroutine(target, 0f, 1f, duration));
    }

    private IEnumerator FadeCoroutine(GameObject target, float startAlpha, float endAlpha, float duration)
    {
        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = target.AddComponent<CanvasGroup>();
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = endAlpha;
    }
    
    private IEnumerator HideAfterDelay(GameObject target, float delay)
    {
        yield return new WaitForSeconds(delay);
        FadeOut(target, 0.3f);
        yield return new WaitForSeconds(0.3f);
        target.SetActive(false);
    }
}

    