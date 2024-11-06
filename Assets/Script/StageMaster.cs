using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스테이지 별 게임 환경을 설정하는 스크립트. 적과 아군의 배치, 스테이지의 패베 및 승리 조건, gameManager에 스테이지 관련 데이터 보내기 등, 스테이지를 불러올때 가장 우선적으로 작동하여 스테이지 환경을 구성하는 스크립트이다.
/// </summary>
public class StageMaster : MonoBehaviour
{
    [Header("스테이지 별 게임 환경을 설정하는 스크립트 스테이지를 불러올때 가장 우선적으로 작동하는 스크립트이다. (적 유닛 종류 등)")]

    [SerializeField]
    private StageManager gameManager;

    /// <summary>
    /// 플레이어가 사용 가능한 유닛 슬롯의 카운트
    /// </summary>
    [SerializeField]
    private int playerUseUnitSlotCount;

    /// <summary>
    /// 플레이어가 사용 가능한 유닛 슬롯의 범위
    /// </summary>
    [SerializeField]
    private int playerUseUnitSlotRange;

    /// <summary>
    /// 해당 스테이지에서 플레이어가 사용 가능하도록 배치되어 있는 유닛 리스트
    /// </summary>
    [SerializeField]
    private List<UnitState> playerUnitList;

    /// <summary>
    /// 해당 스테이지에서 적으로 등장하는 유닛 리스트
    /// </summary>
    [SerializeField]
    private List<UnitState> enemyUnitList;

    /// <summary>
    /// 스킬 데이터를 플레이어에게 보여주는 UI 슬롯 리스트
    /// </summary>
    [SerializeField]
    private List<SkillSlotUIController> skillSlotList;


    /// <summary>
    /// 인 게임에서 유닛이 존재하는 슬롯의 그룹
    /// </summary>
    [SerializeField]
    private UnitSlotGroupController unitSlotGroupController;

    /// <summary>
    /// 플레이어가 배치 가능한 유닛이 존재하는 슬롯의 그룹
    /// </summary>
    [SerializeField]
    private UnitSelectSlotGroupController unitSelectSlotsGroupController;


    private void Start()
    {
        gameManager = StageManager.Instance;

        unitSelectSlotsGroupController.SetUnitSelectSlot();

        PlaceUnitSlot();

        gameManager.playerUseUnitSlotCount = playerUseUnitSlotCount;
        gameManager.playerUseUnitSlotRange = playerUseUnitSlotRange;

        gameManager.skillSlotList = skillSlotList;
        Debug.Log($"skillSlotList.Count {skillSlotList.Count}\n skillSlotList[0] : {skillSlotList[0]}");
        Debug.Log($"gameManager.skillSlotList.Count {gameManager.skillSlotList.Count}\n gameManager.skillSlotList[0] : {gameManager.skillSlotList[0]}");

         
        gameManager.SetGame();
    }

    /// <summary>
    /// 스테이지에 위치한 유닛들을 배치합니다.
    /// </summary>
    private void PlaceUnitSlot()
    {
        int startNum = 0;
        for (int i = 0; i < playerUnitList.Count; i++)
        {
            unitSlotGroupController.unitSlots[startNum + i].ChangeUnit(playerUnitList[i], 1);
        }

        int endNum = unitSlotGroupController.unitSlots.Count - 1;
        for (int i = 0; i < enemyUnitList.Count; i++)
        {
            unitSlotGroupController.unitSlots[endNum - i].ChangeUnit(enemyUnitList[i], 2);
        }
    }
}
