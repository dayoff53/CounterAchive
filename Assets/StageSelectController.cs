using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

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
    private GameObject stageButtonList;

    [SerializeField]
    private List<GameObject> stageSequence;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dataManager = DataManager.Instance;

        int selectStageLength = dataManager.currentSaveData.lastStageNumber;

        stageSequence.Add(firstStageButton);
        //시작 버튼과 마지막 버튼을 제외한 스테이지 버튼 생성
        for (int i = 0; i < selectStageLength - 2; i++)
        {
            stageSequence.Add(Instantiate(stageButtonList, backgroundObject.transform));
        }
        stageSequence.Add(lastStageButton);

        for (int i = 0; i < stageSequence.Count; i++)
        {
            stageSequence[i].transform.SetSiblingIndex(i);
        }

        selectStageLength -= 2;

        if (selectStageLength < 0)
            selectStageLength = 0;

        backgroundObject.GetComponent<RectTransform>().sizeDelta = new Vector2((selectStageLength * 640) + 2560, 1440);
    }
}
