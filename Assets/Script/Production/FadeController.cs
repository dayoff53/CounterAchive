using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    [SerializeField] private Image fadeImage; // 페이드를 적용할 이미지
    [SerializeField] private CanvasGroup fadeCanvasGroup; // 페이드를 적용할 캔버스 그룹

    private void Awake()
    {
        // Image 컴포넌트가 할당되어 있지 않다면, 자기 자신의 게임 오브젝트에서 찾음
        fadeImage = GetComponent<Image>();

        if (fadeImage == null)
        {
            Debug.LogWarning($"'{gameObject.name}'에 'Image Component'가 할당되어있지 않습니다.");
        }
        if (fadeCanvasGroup == null)
        {
            Debug.LogWarning($"'{gameObject.name}'에 'Fade CanvasGroup'가 할당되어있지 않습니다.");
        }
    }

    /// <summary>
    /// 페이드 작업을 시작합니다.
    /// </summary>
    /// <param name="isFadeInOut">페이드 인 여부. true면 페이드 인, false면 페이드 아웃.</param>
    /// <param name="fadeTime">페이드가 진행되는 시간.</param>
    /// <param name="fadeColor">페이드 색상.</param>
    public void FadeStart(bool isFadeInOut, float fadeTime, Color fadeColor)
    {
        float startAlpha = isFadeInOut ? 1 : 0;
        float endAlpha = isFadeInOut ? 0 : 1;

        StartCoroutine(FadeCoroutine(startAlpha, endAlpha, fadeTime, fadeColor));
    }


    /// <summary>
    /// 페이드 작업을 시작합니다. 일정시간 이후 기존 색상으로 변환합니다.
    /// </summary>
    /// <param name="isFadeInOut">페이드 인 여부. true면 페이드 인, false면 페이드 아웃.</param>
    /// <param name="fadeTime">페이드가 진행되는 시간.</param>
    /// <param name="fadeColor">페이드 색상.</param>
    public void FadeHoldStart(bool isFadeInOut, float fadeTime, float holdTime, Color fadeColor)
    {
        float startAlpha = isFadeInOut ? 1 : 0;
        float endAlpha = isFadeInOut ? 0 : 1;

        StartCoroutine(FadeHoldCoroutine(startAlpha, endAlpha, fadeTime, holdTime, fadeColor));
    }

    #region FadeCoroutine
    private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, float fadeTime, Color fadeColor)
    {
        //if (fadeImage == null) yield break; // Image 컴포넌트가 없으면 페이드 중단
        
        float elapsedTime = 0;

        Color startColor = fadeColor;
        startColor.a = startAlpha;

        Color endColor = fadeColor;
        endColor.a = endAlpha;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.unscaledDeltaTime;

            if (fadeImage != null)
            {
                fadeImage.color = Color.Lerp(startColor, endColor, elapsedTime / fadeTime);
            }
            if (fadeCanvasGroup != null)
            {
                //Debug.Log($"fadeCanvasGroup.alpha : {fadeCanvasGroup.alpha}");
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeTime);
            }

            yield return null;
        }

        if (fadeImage != null)
        {
            fadeImage.color = endColor;
        }
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = endColor.a;
        }
    }
    private IEnumerator FadeHoldCoroutine(float startAlpha, float endAlpha, float fadeTime, float holdTime, Color fadeColor)
    {
        //if (fadeImage == null) yield break; // Image 컴포넌트가 없으면 페이드 중단

        float elapsedTime = 0;

        Color startColor = fadeColor;
        startColor.a = startAlpha;

        Color endColor = fadeColor;
        endColor.a = endAlpha;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.unscaledDeltaTime;

            if (fadeImage != null)
            {
                fadeImage.color = Color.Lerp(startColor, endColor, elapsedTime / fadeTime);
            }
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeTime);
            }
            yield return null;
        }

        elapsedTime = 0;

        while (elapsedTime < holdTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;

            if (fadeImage != null)
            {
                fadeImage.color = Color.Lerp(endColor, startColor, elapsedTime / fadeTime);
            }
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = Mathf.Lerp(endAlpha, startAlpha, elapsedTime / fadeTime);
            }
            yield return null;
        }


        if (fadeImage != null)
        {
            fadeImage.color = startColor;
        }
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = startColor.a;
        }
    }
    #endregion
}
