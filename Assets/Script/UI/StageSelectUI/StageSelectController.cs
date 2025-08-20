using System.Collections.Generic;
//using Unity.Android.Gradle.Manifest;
using UnityEngine;
using Sirenix.OdinInspector;

public class StageButtonState
{
    [InfoBox("버튼 오브젝트")]
    public GameObject buttonObject;

    [InfoBox("버튼의 인덱스")]
    public int buttonIndex;
    
    [InfoBox("버튼의 레벨")]
    public int buttonLevel;
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
    [DetailedInfoBox("스테이지 번들 오브젝트 프리팹", "StageButton들을 열로 묶어놓을 프리팹")]
    private GameObject stageBundlePrefab;

    [SerializeField]
    [ReadOnly]
    [DetailedInfoBox("스테이지 번들 리스트", "4개의 Stage선택지 묶음 오브젝트들의 리스트")]
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
                    buttonIndex = i,
                    buttonLevel = stageLoadButtonController.stageLevel
                };
                stageLoadBundleListController.stageLoadButtons.Add(stageLoadButtonController);
                stageLoadButtonList.Add(stageButtonState);
            }
            stageBundleList.Add(setStageBundle);
        }

        // 마지막 버튼을 배경 오브젝트에 추가
        stageBundleList.Add(lastStageButton);
        StageLoadButtonListController lastStageLoadBundleListController = lastStageButton.GetComponent<StageLoadButtonListController>();
        lastStageLoadBundleListController.Shuffle();

        /*
                if (dataManager.currentSaveData.lastStageNumber <= dataManager.currentSaveData.currentStageNumber)
                {
                    lastStageLoadBundleListController.SetActiveStageLoadButton(false);
                }
                else
                {
                    lastStageLoadBundleListController.SetActiveStageLoadButton(true);
                }
        */

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
    }
}
