using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// StageLoadBundleListController
/// </summary>
public class StageLoadBundleListController : MonoBehaviour
{
    public List<StageLoadButtonController> StageBundles;


    void Start()
    {
        //Init();
    }

    public void Init()
    {
        for (var i = 0; i < StageBundles.Count; i++)
        {
            StageBundles[i].stageType = (StageType)Random.Range(2, System.Enum.GetValues(typeof(StageType)).Length - 2);
            StageBundles[i].Init();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index">변경할 StageLoadButton의 Index</param>
    /// <param name="stageType">변경할 StageType</param>
    public void SetStageLoadButton(int stageLoadButtonNumber, StageType stageType)
    {
        StageBundles[stageLoadButtonNumber].stageType = stageType;
        StageBundles[stageLoadButtonNumber].Init();
    }

    public void SetActiveStageLoadButton(bool isActive)
    {
        for (var i = 0; i < StageBundles.Count; i++)
        {
            // 또는 버튼만 클릭 가능/불가능하게 하려면:
            Button button = StageBundles[i].GetComponent<Button>();
            if (button != null) button.interactable = isActive;
        }
    }
}
