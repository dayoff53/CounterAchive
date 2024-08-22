using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageMaster : MonoBehaviour
{
    [SerializeField]
    private List<UnitData> playerUnitList;
    [SerializeField]
    private int playerUseUnitSlotCount;
    [SerializeField]
    private List<UnitData> enemyUnitList;

    [SerializeField]
    private GameManager gameManager;
    [SerializeField]
    private UnitSlotGroupController unitSlotGroupController;


    private void Awake()
    {
        gameManager = GameManager.Instance;

        unitSlotGroupController.EnemyUnitInit(enemyUnitList);
        gameManager.playerUseUnitSlotCount = playerUseUnitSlotCount;



        gameManager.StartGame();
    }
}
