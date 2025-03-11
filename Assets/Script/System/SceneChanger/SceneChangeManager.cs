using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneChangeManager : Singleton<SceneChangeManager>
{
	[Header("Scene 변경 총괄 메니저")]

	[SerializeField]
	private string loadSceneName;

	private GameObject deactiveObject;
	private GameObject loadingBar;
	private Image loadingBarImage;
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


		deactiveObject = GameObject.Find("DeactiveObject");
		loadingBar = GameObject.Find("LoadingBar");
		loadingBarImage = loadingBar.GetComponent<Image>();
		loadingBarImage.fillAmount = 0f;

        Debug.Log($"1    !asyncOperation.isDoneb : {!asyncOperation.isDone}");

        float timer = 0f; 
		float duration = 1f;
		float progressTarget = 0f;

        yield return loadingWaitForSeconds;

        Debug.Log($"2   !asyncOperation.isDoneb : {!asyncOperation.isDone}");
        while (!asyncOperation.isDone)
		{
            Debug.Log($"3   !asyncOperation.isDoneb : {!asyncOperation.isDone}"); 
			yield return null;

			timer += Time.unscaledDeltaTime;
			
			if (loadingBarImage.fillAmount < 0.89f)
			{
				progressTarget = asyncOperation.progress;
				loadingBarImage.fillAmount = Mathf.SmoothStep(loadingBarImage.fillAmount, progressTarget, timer / loadingBarSpeed);
			}
			else
			{
				// 0.9에서 1.0까지 부드럽게 증가
				float normalizedTime = (timer - duration) / loadingBarSpeed;
				loadingBarImage.fillAmount = Mathf.Lerp(0.9f, 1f, normalizedTime);

				if (loadingBarImage.fillAmount >= 1f)
				{
					yield return FadeManager.Instance.FadeCoroutineStart(false, 1, Color.black);
					
					// 씬 전환 전에 로딩 UI 상태 유지
					asyncOperation.allowSceneActivation = true;
					
					// 씬 전환 완료 대기
					while (SceneManager.GetActiveScene().name != loadSceneName)
					{
						yield return null;
					}

						// 씬 전환 후 UI 비활성화
						//deactiveObject.SetActive(false);
					
					
						FadeManager.Instance.FadeStart(true, 0.25f, Color.black);
					yield break;
				}
			}

			Debug.Log($"Loading 진행도 : {(int)(loadingBarImage.fillAmount * 100)}");
		}
	}
}