using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StageLoadBundleListController
/// </summary>
public class StageLoadBundleListController : MonoBehaviour
{
    public List<GameObject> StageBundles;

    void Start()
    {
        //Init();
    }

    public void Init()
    {
        foreach (var stageButton in StageBundles)
        {
            StageLoadButtonController stageSelectController = stageButton.GetComponent<StageLoadButtonController>();
            stageSelectController.stageType = (StageType)Random.Range(2, System.Enum.GetValues(typeof(StageType)).Length - 2);
            stageSelectController.Init();
        }
    }

/// <summary>
/// 
/// </summary>
/// <param name="index">변경할 StageLoadButton의 Index</param>
/// <param name="stageType">변경할 StageType</param>
    public void SetStageLoadButton(int stageLoadButtonNumber, StageType stageType)
    {
            StageLoadButtonController stageSelectController = StageBundles[stageLoadButtonNumber].GetComponent<StageLoadButtonController>();
            stageSelectController.stageType = stageType;
            stageSelectController.Init();
    }
}
