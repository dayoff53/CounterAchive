using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 플레이어가 배치 가능한 유닛이 존재하는 슬롯을 총괄하는 스크립트
/// </summary>
public class UnitSelectSlotGroupController : MonoBehaviour
{
    [SerializeField]
    private GameObject selectUnitSlot;
    [SerializeField]
    private GameObject contentObject;

    /// <summary>
    /// 배치할 수 있는 UnitSlot의 List
    /// </summary>
    public List<UnitSelectSlotController> selectUnitSlotList;
    private DataManager dataManager;

    /// <summary>
    /// 플레이어가 배치 가능한 유닛의 슬롯들을 초기화하는 스크립트
    /// </summary>
    public void InitUnitSelectSlot()
    {
        dataManager = DataManager.Instance;
        selectUnitSlotList = new List<UnitSelectSlotController>();

        Debug.Log($"dataManager.playerUnitStateList.Count : {dataManager.playerUnitStateList.Count}");
        for (int i = 0; i < dataManager.playerUnitStateList.Count; i++)
        {
            GameObject currentSelectUnitSlot = Instantiate(selectUnitSlot, contentObject.transform);
            selectUnitSlotList.Add(currentSelectUnitSlot.GetComponent<UnitSelectSlotController>());
            selectUnitSlotList[i].unitState = dataManager.playerUnitStateList[i];
            selectUnitSlotList[i].Init();

            Debug.Log($"selectUnitSlotList.Count : {selectUnitSlotList.Count}");
        }
    }
}
