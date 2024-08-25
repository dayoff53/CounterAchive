using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스테이지 별 게임 환경을 설정하는 스크립트. (적 유닛 종류 등)
/// </summary>
public class StageMaster : MonoBehaviour
{
    /// <summary>
    /// 해당 스테이지에서 플레이어가 사용 가능한 유닛 리스트
    /// </summary>
    [SerializeField]
    private List<UnitData> playerUnitList;

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
    /// 해당 스테이지에서 적으로 등장하는 유닛 리스트
    /// </summary>
    [SerializeField]
    private List<UnitData> enemyUnitList;

    [SerializeField]
    private GameManager gameManager;
    [SerializeField]
    private UnitSlotGroupController unitSlotGroupController;


    private void Start()
    {
        gameManager = GameManager.Instance;

        unitSlotGroupController.EnemyUnitInit(enemyUnitList);
        gameManager.playerUseUnitSlotCount = playerUseUnitSlotCount;
        gameManager.playerUseUnitSlotRange = playerUseUnitSlotRange;



        gameManager.SetGame();
    }
}
