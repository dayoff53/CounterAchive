using System.Collections.Generic;
using System.Linq;  
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


[DetailedInfoBox("StageSelectUI사용 시 Stage들의 열을 담당하는 오브젝트 컨트롤러", "StageLoadBundleController는 여러 개의 StageLoadButtonController를 관리하는 컨트롤러입니다. \n스테이지 버튼의 초기화 및 셔플 기능과 활성/비활성 기능을 제공합니다.")]
public class StageLoadButtonListController : MonoBehaviour
{
    public List<StageLoadButtonController> stageLoadButtons;


    void Start()
    {
        //Init();
    }

    public void Shuffle()
    {
        for (var i = 0; i < stageLoadButtons.Count; i++)
        {
            if (stageLoadButtons[i].stageType != StageType.Sensei && stageLoadButtons[i].stageType != StageType.LastBoss)
            {
                stageLoadButtons[i].stageType = (StageType)UnityEngine.Random.Range(2, System.Enum.GetValues(typeof(StageType)).Length - 2);
                stageLoadButtons[i].Init();
            }
        }
    }

    /// <summary>
    /// 특정 StageLoadButton의 StageType을 변경합니다.
    /// </summary>
    /// <param name="index">변경할 StageLoadButton의 Index</param>
    /// <param name="stageType">변경할 StageType</param>
    public void SetStageLoadButton(int stageLoadButtonNumber, StageType stageType)
    {
        stageLoadButtons[stageLoadButtonNumber].stageType = stageType;
        stageLoadButtons[stageLoadButtonNumber].Init();
    }

/// <summary>
/// 해당 StageLoadButtons들의 활성화 상태를 변경합니다.
/// </summary>
/// <param name="isActive"></param>
    public void SetActiveStageLoadButton(bool isActive)
    {
        for (var i = 0; i < stageLoadButtons.Count; i++)
        {
            Button button = stageLoadButtons[i].GetComponent<Button>();
            if (button != null) button.interactable = isActive;
            button.image.color = isActive ? Color.white : Color.gray;
        }
    }
}
