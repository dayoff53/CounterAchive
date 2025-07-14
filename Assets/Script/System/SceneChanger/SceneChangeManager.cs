using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneChangeManager : Singleton<SceneChangeManager>
{
    [Header("씬 전환 설정")]
    [SerializeField] private string loadSceneName;
    [SerializeField] private SceneKeyData loadSceneKey;
    [SerializeField] private float loadingBarSpeed = 1f;
    [SerializeField] private float loadingDelayTime = 0.25f;

    private GameObject deactiveObject;
    private GameObject loadingBar;
    private Image loadingBarImage;
    private WaitForSeconds loadingWaitForSeconds;

    private void Start()
    {
        loadingWaitForSeconds = new WaitForSeconds(loadingDelayTime);
    }

    public void SceneLoad(SceneKeyData sceneKey)
    {
        loadSceneKey = sceneKey;
        loadSceneName = sceneKey.sceneName;
        
        CoroutineManager.Instance.StopCoroutines();
        SceneManager.LoadScene("LoadingScene");
        
        ResetTimeScale();
        StartCoroutine(LoadSceneProcess());
    }

    public void SelfSceneLoad()
    {
        var sceneKeyData = new SceneKeyData
        {
            sceneName = SceneManager.GetActiveScene().name
        };
        SceneLoad(sceneKeyData);
    }

    private void ResetTimeScale()
    {
        if (Time.timeScale != 1f)
        {
            Time.timeScale = 1f;
        }
    }

    private void InitializeLoadingUI()
    {
        deactiveObject = GameObject.Find("DeactiveObject");
        loadingBar = GameObject.Find("LoadingBar");
        loadingBarImage = loadingBar.GetComponent<Image>();
        loadingBarImage.fillAmount = 0f;
    }

    private IEnumerator LoadSceneProcess()
    {
        yield return FadeManager.Instance.FadeCoroutineStart(true, 1, Color.black);

        while (SceneManager.GetActiveScene().name != "LoadingScene")
        {
            yield return null;
        }

        var asyncOperation = SceneManager.LoadSceneAsync(loadSceneName);
        asyncOperation.allowSceneActivation = false;

        InitializeLoadingUI();

        float timer = 0f;
        float duration = 1f;

        yield return loadingWaitForSeconds;

        while (!asyncOperation.isDone)
        {
            yield return null;
            timer += Time.unscaledDeltaTime;

            UpdateLoadingBar(timer, duration, asyncOperation);

            if (loadingBarImage.fillAmount >= 1f)
            {
                yield return CompleteLoading(asyncOperation);
                yield break;
            }
        }
    }

    private void UpdateLoadingBar(float timer, float duration, AsyncOperation asyncOperation)
    {
        if (loadingBarImage.fillAmount < 0.89f)
        {
            float progressTarget = asyncOperation.progress;
            loadingBarImage.fillAmount = Mathf.SmoothStep(loadingBarImage.fillAmount, progressTarget, timer / loadingBarSpeed);
        }
        else
        {
            float normalizedTime = (timer - duration) / loadingBarSpeed;
            loadingBarImage.fillAmount = Mathf.Lerp(0.9f, 1f, normalizedTime);
        }

        Debug.Log($"로딩 진행도: {(int)(loadingBarImage.fillAmount * 100)}%");
    }

    private IEnumerator CompleteLoading(AsyncOperation asyncOperation)
    {
        yield return FadeManager.Instance.FadeCoroutineStart(false, 1, Color.black);
        
        asyncOperation.allowSceneActivation = true;
        
        while (SceneManager.GetActiveScene().name != loadSceneName)
        {
            yield return null;
        }

        FadeManager.Instance.FadeStart(true, 0.25f, Color.black);
    }
}