using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;


/// <summary>
/// 화면 전체의 페이드인과 페이드아웃을 제어하는 클래스입니다.
/// </summary>
[Singleton(Automatic = true, Persistent = true, Name = "FadeManager", HideFlags = HideFlags.None)]

public class FadeManager : Singleton<FadeManager>
{
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private float aspectRatio;  //현재 화면의 해상도
    private RectTransform rectTransform;

    private Color fadeColor = Color.black;
    private float fadeTime = 1.0f;
    private GameObject fadeObject;
    private Image fadeImage;
    private Coroutine currentFadeCoroutine;



    private void Awake()
    {
        base.Awake();

        if (canvasGroup == null)
        {
            GameObject fadeCanvas = new GameObject("FadeCanvas");
            fadeCanvas.transform.SetParent(transform);

            canvas = fadeCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = int.MaxValue * -1;

            canvasGroup = fadeCanvas.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0;

            //rectTransform 초기화(화면 전체 할당)
            rectTransform = fadeCanvas.GetComponent<RectTransform>();
            FullRectTransform(rectTransform);

            aspectRatio = rectTransform.rect.width / rectTransform.rect.height;

            if (!fadeImage || !fadeObject)
            {
                fadeObject = new GameObject("fadeObject");
                fadeObject.transform.parent = fadeCanvas.transform;
                FullRectTransform(fadeObject.AddComponent<RectTransform>());
                fadeImage = fadeObject.AddComponent<Image>();
            }
            
            /*
            if (!irisoutImage || !irisoutObject)
            {
                irisoutObject = new GameObject("irisoutObject");
                irisoutObject.transform.parent = fadeCanvas.transform;
                FullRectTransform(irisoutObject.AddComponent<RectTransform>());
                irisoutImage = irisoutObject.AddComponent<Image>();
                irisoutImage.material = ResourceManager.Instance.Load<Material>("IrisOut");

                irisoutImage.material.SetFloat("_AspectRatio", aspectRatio);
                irisoutImage.material.SetFloat("_IrisRadius", 500f);
            }
            */

            fadeImage.color = fadeColor;
        }
    }

    private void FullRectTransform(RectTransform fullingRectTransform)
    {
        fullingRectTransform.anchorMin = Vector2.zero;
        fullingRectTransform.anchorMax = Vector2.one;
        fullingRectTransform.sizeDelta = Vector2.zero;
        fullingRectTransform.anchoredPosition = Vector2.zero;
    }


    #region FadeInOut
    /// <summary>
    /// 페이드를 시작하는 함수입니다. 인자로 페이드인, 페이드아웃 여부, 페이드 시간, 페이드 색상을 받습니다.
    /// </summary>
    /// <param name="isFadeInOut">페이드인 여부를 설정합니다. true면 페이드인, false면 페이드아웃을 수행합니다.</param>
    /// <param name="fadeTime">페이드가 이루어지는 동안의 시간을 설정합니다.</param>
    /// <param name="fadeColor">페이드의 색상을 설정합니다.</param>
    public void FadeStart(bool isFadeInOut, float fadeTime, Color fadeColor)
    {
        this.fadeTime = fadeTime;
        this.fadeColor = fadeColor;
        fadeImage.color = this.fadeColor;

        float startAlpha = 0;
        float endAlpha = 0;
        if (isFadeInOut)
        {
            startAlpha = 1;
        }
        else
        {
            endAlpha = 1;
        }


        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }


        currentFadeCoroutine = StartCoroutine(FadeCoroutine(startAlpha, endAlpha));
    }


    /// <summary>
    /// 페이드를 시작하는 함수입니다. 인자로 페이드인, 페이드아웃 여부, 페이드 시간, 페이드 색상을 받습니다.
    /// </summary>
    /// <param name="isFadeInOut">페이드인 여부를 설정합니다. true면 페이드인, false면 페이드아웃을 수행합니다.</param>
    /// <param name="fadeTime">페이드가 이루어지는 동안의 시간을 설정합니다.</param>
    /// <param name="fadeColor">페이드의 색상을 설정합니다.</param>
    public IEnumerator FadeCoroutineStart(bool isFadeInOut, float fadeTime, Color fadeColor)
    {
        this.fadeTime = fadeTime;
        this.fadeColor = fadeColor;
        fadeImage.color = this.fadeColor;

        float startAlpha = 0;
        float endAlpha = 0;
        if (isFadeInOut)
        {
            startAlpha = 1;
        }
        else
        {
            endAlpha = 1;
        }


        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }


        return FadeCoroutine(startAlpha, endAlpha);
    }


    /// <summary>
    /// 페이드 코루틴 함수입니다.
    /// </summary>
    /// <param name="startAlpha">페이드가 시작될 때의 알파값을 설정합니다.</param>
    /// <param name="endAlpha">페이드가 끝날 때의 알파값을 설정합니다.</param>
    private IEnumerator FadeCoroutine(float startAlpha, float endAlpha)
    {
        canvasGroup.alpha = startAlpha;
        canvasGroup.blocksRaycasts = true;

        float elapsedTime = 0;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeTime);

            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        canvasGroup.blocksRaycasts = false;

        currentFadeCoroutine = null;
    }


    /// <summary>
    ///  페이드를 취소하는 함수 입니다.
    /// </summary>
    public void CancelFade()
    {
        if (currentFadeCoroutine != null)
        {
            // 현재 실행 중인 코루틴을 중지합니다.
            StopCoroutine(currentFadeCoroutine);
            currentFadeCoroutine = null;

            // 알파값과 레이 캐스팅 블록을 초기화합니다.
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
        }
    }
    #endregion


    #region Irisout

    /*
    /// <summary>
    /// IrisoutStart을 시작합니다.
    /// </summary>
    /// <param name="target">Irisout의 중심이 될 타겟</param>
    /// <param name="delayTime">Irisout을 시작하기 전까지의 딜레이</param>
    /// <returns>Irisout이 종료될때까지의 시간</returns>
    public float IrisoutStart(Transform target, float delayTime)
    {
        Debug.Log($"IrisoutStart");

        irisoutCurrentRadius = 1.0f;
        irisoutTargetTransform = target;

        StartCoroutine(IrisoutCoroutine(delayTime));

        return (irisoutCurrentRadius / irisoutSpeed) + delayTime;
    }


    public IEnumerator IrisoutCoroutine(float delayTime)
    {
        Debug.Log($"IrisoutCoroutine WaitForSeconds. . . ");
        yield return CachedYield.WaitForSecondsRealtime(delayTime);
        Debug.Log($"IrisoutCoroutine");

        Vector3 screenPos = Camera.main.WorldToScreenPoint(irisoutTargetTransform.position);
        Vector2 normalizedPos = new Vector2(screenPos.x / Screen.width, screenPos.y / Screen.height);
        irisoutImage.material.SetVector("_IrisCenter", new Vector4(normalizedPos.x * aspectRatio, normalizedPos.y, 0, 0));


        while (irisoutCurrentRadius > 0)
        {
            irisoutCurrentRadius -= Time.unscaledDeltaTime * irisoutSpeed;
            irisoutImage.material.SetFloat("_IrisRadius", irisoutCurrentRadius);

            yield return null;
        }
    }
    */
    #endregion

}