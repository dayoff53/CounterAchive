using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Sirenix.OdinInspector;

[DetailedInfoBox("페이드 효과를 제어하는 컨트롤러", "UI 이미지나 캔버스 그룹, 파티클 시스템에 페이드 인/아웃 효과를 적용하는 컨트롤러입니다.")]
public class FadeController : MonoBehaviour
{
    [SerializeField]
    [Title("Fade Components")]
    [InfoBox("페이드를 적용할 이미지")]
    private Image fadeImage;

    [SerializeField] 
    [InfoBox("페이드를 적용할 캔버스 그룹")]
    private CanvasGroup fadeCanvasGroup;

    [SerializeField]
    [InfoBox("페이드를 적용할 파티클 시스템")]
    private ParticleSystem fadeParticleSystem;

    [Title("Inspector Fade Settings (인스팩터 창에서 페이드 인엔아웃 기능을 Event로 사용할 수 있도록 하기위한 변수)")]
    [SerializeField]
    [FoldoutGroup("인스팩터 페이드 설정")]
    [InfoBox("페이드 인/아웃 여부 (true: 페이드 인, false: 페이드 아웃)")]
    private bool isFadeIn;

    [SerializeField]
    [FoldoutGroup("인스팩터 페이드 설정")]
    [InfoBox("페이드 시간")]
    private float inspectorFadeTime = 1f;

    [SerializeField]
    [FoldoutGroup("인스팩터 페이드 설정")]
    [InfoBox("페이드 색상")]
    private Color inspectorFadeColor = Color.black;

    [Button("페이드 실행")]
    [FoldoutGroup("인스팩터 페이드 설정")]
    public void ExecuteFade(bool isFadeOut)
    {
        FadeStart(isFadeOut, inspectorFadeTime, inspectorFadeColor);
    }

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
        if (fadeParticleSystem == null)
        {
            Debug.LogWarning($"'{gameObject.name}'에 'Particle System'이 할당되어있지 않습니다.");
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
        float startAlpha = isFadeInOut ? fadeColor.a : 0;
        float endAlpha = isFadeInOut ? 0 : fadeColor.a;

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
        float startAlpha = isFadeInOut ? fadeColor.a : 0;
        float endAlpha = isFadeInOut ? 0 : fadeColor.a;

        StartCoroutine(FadeHoldCoroutine(startAlpha, endAlpha, fadeTime, holdTime, fadeColor));
    }

    #region FadeCoroutine
    private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, float fadeTime, Color fadeColor)
    {
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
            if (fadeParticleSystem != null)
            {
                var main = fadeParticleSystem.main;
                main.startColor = Color.Lerp(startColor, endColor, elapsedTime / fadeTime);
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
        if (fadeParticleSystem != null)
        {
            var main = fadeParticleSystem.main;
            main.startColor = endColor;
        }
    }

    private IEnumerator FadeHoldCoroutine(float startAlpha, float endAlpha, float fadeTime, float holdTime, Color fadeColor)
    {
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
            if (fadeParticleSystem != null)
            {
                var main = fadeParticleSystem.main;
                main.startColor = Color.Lerp(startColor, endColor, elapsedTime / fadeTime);
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
            if (fadeParticleSystem != null)
            {
                var main = fadeParticleSystem.main;
                main.startColor = Color.Lerp(endColor, startColor, elapsedTime / fadeTime);
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
        if (fadeParticleSystem != null)
        {
            var main = fadeParticleSystem.main;
            main.startColor = startColor;
        }
    }
    #endregion
}
