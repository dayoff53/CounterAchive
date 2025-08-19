using System.Collections.Generic;
using System.Linq;  
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


[DetailedInfoBox("StageSelectUI사용 시 Stage들의 열을 담당하는 오브젝트 컨트롤러", "StageLoadBundleController는 여러 개의 StageLoadButtonController를 관리하는 컨트롤러입니다. \n스테이지 버튼의 초기화 및 셔플 기능과 활성/비활성 기능을 제공합니다.")]
public class StageLoadBundleController : MonoBehaviour
{
    public List<StageLoadButtonController> StageBundles;


    void Start()
    {
        //Init();
    }

    public void Shuffle()
    {
        for (var i = 0; i < StageBundles.Count; i++)
        {
            if (StageBundles[i].stageType != StageType.Sensei && StageBundles[i].stageType != StageType.LastBoss)
            {
                StageBundles[i].stageType = (StageType)UnityEngine.Random.Range(2, System.Enum.GetValues(typeof(StageType)).Length - 2);
                StageBundles[i].Init();
            }
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
