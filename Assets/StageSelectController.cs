using System.Collections.Generic;
//using Unity.Android.Gradle.Manifest;
using UnityEngine;
using Sirenix.OdinInspector;


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
    [DetailedInfoBox("스테이지 번들 오브젝트 프리팹", "4개의 Stage선택지 묶음 오브젝트 프리팹")]
    private GameObject stageBundle;

    [SerializeField]
    [ReadOnly]
    [DetailedInfoBox("스테이지 번들 리스트", "4개의 Stage선택지 묶음 오브젝트들의 리스트")]
    private List<GameObject> stageBundleList;


    void Start()
    {
        dataManager = DataManager.Instance;
        Init();
    }

    public void Init()
    {
        int selectStageLength = dataManager.currentSaveData.lastStageNumber - 2; // -2는 시작과 마지막 버튼을 제외시키기 위함

        stageBundleList.Add(firstStageButton);
        //시작 버튼과 마지막 버튼을 제외한 스테이지 버튼 생성
        for (int i = 0; i < selectStageLength; i++)
        {
            GameObject setStageBundle = Instantiate(stageBundle, backgroundObject.transform);
            StageLoadBundleListController stageLoadBundleListController = setStageBundle.GetComponent<StageLoadBundleListController>();
            stageLoadBundleListController.Init();
            stageLoadBundleListController.SetActiveStageLoadButton(false);
            stageBundleList.Add(setStageBundle);
        }

        // 마지막 버튼을 배경 오브젝트에 추가
        lastStageButton = Instantiate(lastStageButton, backgroundObject.transform);
        StageLoadBundleListController lastStageLoadBundleListController = lastStageButton.GetComponent<StageLoadBundleListController>();
        lastStageLoadBundleListController.Init();
        stageBundleList.Add(lastStageButton);

        if (dataManager.currentSaveData.lastStageNumber <= dataManager.currentSaveData.currentStageNumber)
        {
            lastStageLoadBundleListController.SetActiveStageLoadButton(false);
        }
        else
        {
            lastStageLoadBundleListController.SetActiveStageLoadButton(true);
        }

        if (selectStageLength < 0)
        {
            selectStageLength = 0;
        }

        backgroundObject.GetComponent<RectTransform>().sizeDelta = new Vector2((selectStageLength * 640) + 2560, 1440);
        
        
        // 스테이지 진행도 적용
        for (int i = 0; i < stageBundleList.Count; i++)
        {
            Debug.Log($"{stageBundleList[i].name} index = stageBundleList[{i}]");
            stageBundleList[i].transform.SetSiblingIndex(i);
            stageBundleList[i].GetComponent<StageLoadBundleListController>().SetActiveStageLoadButton(true); // StageType은 2부터 시작
        }
    }
}
