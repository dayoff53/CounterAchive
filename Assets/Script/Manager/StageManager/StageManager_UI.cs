using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public partial class StageManager
{
    #region UIVariable
    [Space(10)]
    [Header("UI Object")]
    public GameObject play_UI;
    public GameObject unitSet_UI;
    public UnitCard turnUnitCardUI;
    public UnitCard targetUnitCardUI;
    public TMP_Text skillAccuracyText;
    public TMP_Text remainingSetUnitSlotText;
    public GameObject turnUnitMarker;
    public GameObject targetUnitMarker;


    [Space(10)]
    [Header("Color Data")]
    public List<Color> unitStateColors;
    public ColorState unitStateColorsObject;
    #endregion


    /// <summary>
    /// 인 게임의 현 상황을 보여주는 UnitCardUI를 변경하는 스크립트
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
}
