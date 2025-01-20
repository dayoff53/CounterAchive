using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 클릭시 유닛을 선택하는 기능을 수행하며, UnitStatus를 반환하는 역할을 함
/// </summary>
public class UnitSelectController : MonoBehaviour
{
    [SerializeField]
    public UnitStatus unitStatus;

    [SerializeField]
    private StageMaster stageManager;


    //UnitSelectSlot이 처음 호출 되었을 경우 Null상태일 때, 유닛 데이터가 세팅되었을때 다시 호출 됨
    public void Init(StageMaster stageManager)
    {
        this.stageManager = stageManager;
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }


    private void OnButtonClick()
    {
        stageManager.currentSelectUnitState = unitStatus;
    }
}
