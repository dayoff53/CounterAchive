 using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneChangeManager : Singleton<SceneChangeManager>
{
	[Header("Scene 변경 총괄 메니저")]

	[SerializeField]
	private string loadSceneName;

	[SerializeField]
	private Image loadingBarImage;
	[SerializeField]
	private GameObject loadingBar;
	[SerializeField]
	private float loadingBarSpeed = 1;
	[SerializeField]
	private float loadingDelayTime = 0.25f;
	WaitForSeconds loadingWaitForSeconds;

	[SerializeField]
	private SceneKeyData loadSceneKey;

    public void Start()
	{
		loadingWaitForSeconds = new WaitForSeconds(loadingDelayTime);
	}

	public void SceneLoad(SceneKeyData loadSceneKey)
	{
		this.loadSceneKey = loadSceneKey;
		loadSceneName = loadSceneKey.sceneName;
		
		CoroutineManager.Instance.StopCoroutines();
		
		SceneManager.LoadScene($"LoadingScene");
        
        if (Time.timeScale != 1f)
        {
            Time.timeScale = 1f;
        }

        StartCoroutine(LoadSceneProcess());
	}

	public void SelfSceneLoad()
	{
		//#설명#	자기 자신을 다시 불러오는 함수

		SceneKeyData sceneKeyData = new SceneKeyData();

		sceneKeyData.sceneName = SceneManager.GetActiveScene().name;

		SceneLoad(sceneKeyData);
	}

	IEnumerator LoadSceneProcess()
	{
		yield return FadeManager.Instance.FadeCoroutineStart(true, 1, Color.black);

		//Scene이 불러와졌는지 확인
		while (SceneManager.GetActiveScene().name != $"LoadingScene")
		{
			yield return null;
		}

		//AsyncOperation은 비동기 작업을 처리할때 사용하는 클래스
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(loadSceneName);
		//로딩 씬이 나오기도 전에 로딩이 완료될 수 있기 때문에 로딩이 다 되더라도 일단 멈춰둔다.
		asyncOperation.allowSceneActivation = false;


		loadingBar = GameObject.Find("LoadingBar");
		loadingBarImage = loadingBar.GetComponent<Image>();
		loadingBarImage.fillAmount = 0f;

        Debug.Log($"1    !asyncOperation.isDoneb : {!asyncOperation.isDone}");

        float timer = 0f; 
		float duration = 1f; // SmoothStep 적용 시간, 이 값을 조정하여 부드러운 이동이 얼마나 빠르게 이루어질지 결정

        yield return loadingWaitForSeconds;

        Debug.Log($"2   !asyncOperation.isDoneb : {!asyncOperation.isDone}");
        while (!asyncOperation.isDone)
		{
            Debug.Log($"3   !asyncOperation.isDoneb : {!asyncOperation.isDone}"); 
			yield return null;

			if (loadingBarImage.fillAmount < 0.89f)
			{
				timer += Time.unscaledDeltaTime / duration;
				// SmoothStep을 이용하여 로딩 진행도를 시각적으로 부드럽게 표현
				loadingBarImage.fillAmount = Mathf.SmoothStep(loadingBarImage.fillAmount, asyncOperation.progress, timer / loadingBarSpeed);
			}
			else
			{
				timer += Time.unscaledDeltaTime;
				loadingBarImage.fillAmount = Mathf.Lerp(0.9f, 1f, timer / loadingBarSpeed);


				//Scene의 로딩이 끝날경우 Scene활성화(번경)
				if (loadingBarImage.fillAmount >= 1f)
				{
					yield return FadeManager.Instance.FadeCoroutineStart(false, 1, Color.black);
					FadeManager.Instance.FadeStart(false, 0, Color.black);

					asyncOperation.allowSceneActivation = true;

					while (SceneManager.GetActiveScene().name != loadSceneName)
					{
						loadingBar.SetActive(false);
						FadeManager.Instance.FadeStart(true, 0, Color.black);
						yield break;
					}
				}
			}

			Debug.Log($"Loading 진행도 : {(int)(loadingBarImage.fillAmount * 100)}");
		}
	}
}