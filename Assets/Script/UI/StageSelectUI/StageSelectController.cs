using System.Collections.Generic;
//using Unity.Android.Gradle.Manifest;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class StageButtonState
{
    [InfoBox("버튼 오브젝트")]
    public GameObject buttonObject;

    [InfoBox("버튼의 인덱스")]
    public int[] buttonIndex = new int[2];

    [InfoBox("버튼의 레벨")]
    public int buttonLevel;

    public List<int> nextStageNumbers = new List<int>();
}

[InfoBox("DataManager에 있는 currentSaveData의 lastStageNumber 값을 기준으로 스테이지 시퀀스를 생성합니다.")]
public class StageSelectController : MonoBehaviour
{
    private DataManager dataManager;

    [SerializeField]
    private GameObject backgroundObject;

    [SerializeField]
    private GameObject firstStageButton;
    [SerializeField]
    private GameObject lastStageButton;

    [SerializeField]
    [InfoBox("스테이지 버튼들의 리스트")]
    private List<StageButtonState> stageLoadButtonList;

    [SerializeField]
    [InfoBox("StageButton 오브젝트 프리팹")]
    private GameObject stageButtonPrefab;

    [SerializeField]
    [DetailedInfoBox("스테이지 번들 오브젝트 프리팹", "스테이지 번들 오브젝트 프리팹 \nStageButton들을 열로 묶어놓을 프리팹")]
    private GameObject stageBundlePrefab;

    [SerializeField]
    [ReadOnly]
    [DetailedInfoBox("스테이지 번들 리스트", "스테이지 번들 리스트 \n4개의 Stage선택지 묶음 오브젝트들의 리스트")]
    private List<GameObject> stageBundleList;


    [SerializeField]
    private int selectStageLength = 0; // 현재 선택된 스테이지의 길이

    void Start()
    {
        dataManager = DataManager.Instance;
        Init();
    }

    public void Init()
    {
        if (stageBundleList != null && stageBundleList.Count > 0)
        {
            for (int i = 0; i < stageLoadButtonList.Count; i++)
            {
                Destroy(stageLoadButtonList[i].buttonObject);
            }
        }
        stageLoadButtonList = new List<StageButtonState>();

        selectStageLength = dataManager.currentSaveData.lastStageNumber - -1; // -1는 마지막 버튼을 제외시키기 위함

        // 시작 버튼과 마지막 버튼을 제외한 스테이지 버튼 생성
        stageBundleList.Add(firstStageButton);
        for (int i = 1; i < selectStageLength; i++)
        {
            GameObject setStageBundle = Instantiate(stageBundlePrefab, backgroundObject.transform);
            StageLoadButtonListController stageLoadBundleListController = setStageBundle.GetComponent<StageLoadButtonListController>();
            stageLoadBundleListController.stageLoadButtons = new List<StageLoadButtonController>();

            // 스테이지 버튼들을 생성하고 리스트에 추가
            for (int j = 0; j < 4; j++)
            {
                GameObject stageButton = Instantiate(stageButtonPrefab, stageLoadBundleListController.transform);
                StageLoadButtonController stageLoadButtonController = stageButton.GetComponent<StageLoadButtonController>();
                stageLoadButtonController.stageType = (StageType)UnityEngine.Random.Range(2, System.Enum.GetValues(typeof(StageType)).Length - 2);
                StageButtonState stageButtonState = new StageButtonState
                {
                    buttonObject = stageButton,
                    buttonIndex = new int[] { i, j },
                    buttonLevel = stageLoadButtonController.stageLevel,

                };
                stageLoadButtonController.stageButtonState = stageButtonState;
                stageLoadBundleListController.stageLoadButtons.Add(stageLoadButtonController);
                stageLoadButtonList.Add(stageButtonState);
            }
            stageBundleList.Add(setStageBundle);
        }

        // 마지막 버튼을 배경 오브젝트에 추가
        stageBundleList.Add(lastStageButton);
        StageLoadButtonListController lastStageLoadBundleListController = lastStageButton.GetComponent<StageLoadButtonListController>();
        lastStageLoadBundleListController.Shuffle();

        if (selectStageLength < 0)
        {
            selectStageLength = 0;
        }

        // 배경 오브젝트의 크기를 StageButton 갯수에 알맞게 조정
        backgroundObject.GetComponent<RectTransform>().sizeDelta = new Vector2((selectStageLength * 640) + 2560, 1440);

        // 스테이지 진행도 적용
        for (int i = 0; i < stageBundleList.Count; i++)
        {
            Debug.Log($"stageBundleList[{i}] = {stageBundleList[i].gameObject.name}");
            stageBundleList[i].transform.SetSiblingIndex(i);
            stageBundleList[i].GetComponent<StageLoadButtonListController>().SetActiveStageLoadButton(false); // StageType은 2부터 시작
        }
        for (int i = 0; i <= dataManager.currentSaveData.currentStageNumber; i++)
        {
            stageBundleList[i].GetComponent<StageLoadButtonListController>().SetActiveStageLoadButton(true);
        }
        NextButtonConnecter();
    }
    
    /// <summary>
    /// 다음 스테이지 버튼과 현재 스테이지 버튼을 연결합니다.
    /// </summary>
    void NextButtonConnecter()
    {
        for (int i = 0; i < stageLoadButtonList.Count; i++)
        {
            Debug.Log($"Connecting next buttons for button {i}.");

            int nextBundleIndex = stageLoadButtonList[i].buttonIndex[0] + 1;

            // 기존 값 초기화
            stageLoadButtonList[i].nextStageNumbers.Clear();

            // 다음 번들이 없으면 건너뜀
            if (nextBundleIndex >= stageBundleList.Count) continue;

            // 다음 번들에서 사용 가능한 버튼 수 확인 (available = 사용 가능한 버튼 수)
            StageLoadButtonListController nextBundleController = stageBundleList[nextBundleIndex].GetComponent<StageLoadButtonListController>();
            int available = nextBundleController?.stageLoadButtons?.Count ?? 0;
            if (available == 0) 
            {
                Debug.LogWarning($"No available buttons in the next bundle for button {i}. Skipping connection.");
                continue;
            }else
            {
                Debug.Log($"Next bundle for button {i} has {available} available buttons.");
            }

            // 뽑을 개수(1 ~ 3)를 사용 가능 개수로 제한
            int randomValue = Random.Range(1, 4);
            int take = Mathf.Min(randomValue, available);
            if (take <= 0) Debug.LogWarning($"Take value is non-positive for button {i}. Skipping connection.");

            // 후보 리스트 생성 후 Fisher-Yates로 섞음 (candidates = 후보 리스트)
            List<int> candidates = new List<int>(available);
            for (int k = 0; k < available; k++) candidates.Add(k);
            for (int k = candidates.Count - 1; k > 0; k--)
            {
                int random = Random.Range(0, k + 1);
                int tmp = candidates [k];
                candidates[k] = candidates[random];
                candidates[random] = tmp;
            }


            List<GameObject> LineObjects = stageLoadButtonList[i].buttonObject.GetComponent<StageLoadButtonController>().nextButtonConnectLines;


            
            // 중복 없이 앞에서부터 take개 선택
            for (int j = 0; j < take; j++)
            {
                stageLoadButtonList[i].nextStageNumbers.Add(candidates[j]);
                
                Debug.Log($"Connecting line from button {i} to button {candidates[j]} in next bundle. \nLineObjects.Count = {LineObjects.Count}, LineObjects[{j}] = {LineObjects[j].name}");
                DrawConnectionLine(LineObjects[j],
                    stageLoadButtonList[i].buttonObject.GetComponent<RectTransform>(),
                    nextBundleController.stageLoadButtons[candidates[j]].GetComponent<RectTransform>());
            }
        }
    }

/// <summary>
/// 두 RectTransform 간에 선을 그립니다. (각 스테이지 버튼별로 연결되어있는 버튼끼리 선을 그어 표기함)
/// </summary>
/// <param name="lineObject">선이 될 오브젝트</param>
/// <param name="from">시작 위치</param>
/// <param name="to">끝 위치</param>
    void DrawConnectionLine(GameObject lineObject, RectTransform from, RectTransform to)
    {
        if (lineObject == null || from == null || to == null)
        {
            Debug.LogWarning("Invalid RectTransform provided for line drawing.");
            return;
        }
        else
        {
            Debug.Log($"Drawing line from {from.gameObject.name} to {to.gameObject.name}");
        }


        // 월드 좌표를 로컬 좌표로 변환
        RectTransform lineRect = lineObject.GetComponent<RectTransform>();
        RectTransform parentRect = lineObject.GetComponent<RectTransform>().parent.GetComponent<RectTransform>();
        Vector2 localStartPos;
        Vector2 localEndPos;

        // RectTransformUtility.ScreenPointToLocalPointInRectangle는 스크린 좌표(모니터 픽셀 기준) RectTransform의 로컬 좌표로 변환해주는 함수이다.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            lineRect,
            RectTransformUtility.WorldToScreenPoint(null, from.position),
            null,
            out localStartPos);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            lineRect,
            RectTransformUtility.WorldToScreenPoint(null, to.position),
            null,
            out localEndPos);


        // 선의 위치를 각 목표의 중앙으로 설정
        Vector2 centerPos = (localStartPos + localEndPos) * 0.5f;
        lineRect.localPosition = centerPos;

        Vector2 dir = localEndPos - localStartPos;
        float distance = dir.magnitude;

        // 라인의 길이와 두께 설정 (가로로 긴 막대 + 회전)
        float lineThickness = 1f; // 원하는 두께로 설정
        lineRect.sizeDelta = new Vector2(distance, lineThickness);
        
        // 라인의 회전 설정 
        // 각도 계산 (라디안 -> 도)
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        lineRect.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
