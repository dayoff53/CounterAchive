using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public partial class StageManager
{
    #region UIVariable
    [Space(20)]
    [Header("------------------- UI -------------------")]
    [Header("UI Object")]
    /// <summary>
    /// StageUI 우측 하단에 위치하는 각종 데이터들을 표기하는 공간의 컴포넌트
    /// </summary>
    public StageWindowController stageMenuController;

    public GameObject play_UI;
    public GameObject unitSet_UI;
    public UnitCard turnUnitCardUI;
    public UnitCard targetUnitCardUI;
    public TMP_Text skillAccuracyText;
    public TMP_Text remainingSetUnitSlotText;
    public Image fadeProdutionPanel;


    [Space(10)]
    [Header("Color Data")]
    public List<Color> unitStateColors;
    public ProdutionState unitStateColorsObject;
    #endregion


    /// <summary>
    /// 각 유닛들의 현 상황을 보여주는 UnitCardUI를 관리하는 스크립트
    /// </summary>
    /// <param name="unitNumber"></param>
    public void SetCurrentUnitCardUI(bool isPlayer, int unitNumber)
    {
        if (isPlayer)
        {
            UnitStatus changeUnitStatus = new UnitStatus();
            changeUnitStatus.SetStatus(dataManager.unitDataList.Find(u => u.unitNumber == unitSlotList[unitNumber].unit.unitNumber));
            turnUnitCardUI.unitStatus = changeUnitStatus;
        }
        else
        {
            UnitStatus changeUnitStatus = new UnitStatus();
            changeUnitStatus.SetStatus(dataManager.unitDataList.Find(u => u.unitNumber == unitSlotList[unitNumber].unit.unitNumber));
            targetUnitCardUI.unitStatus = changeUnitStatus;
        }
    }

/// <summary>
/// 
/// </summary>
/// <param name="color"></param>
/// <param name="fadeTime"></param>
    public void SetFadeInOutProduction(Color color, float fadeTime)
    {
        StartCoroutine(FadeInOutProduction(color, fadeTime));
    }
    IEnumerator FadeInOutProduction(Color color, float fadeTime)
    {
        float elapsedTime = 0f;
        Color startColor = fadeProdutionPanel.color;


        if(fadeTime == 0)
        {
            fadeProdutionPanel.color = color;
            yield break;
        }

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeTime;
            fadeProdutionPanel.color = Color.Lerp(startColor, color, t);
            yield return new WaitForSeconds(Time.deltaTime);
        }

        fadeProdutionPanel.color = color;
    }
}
