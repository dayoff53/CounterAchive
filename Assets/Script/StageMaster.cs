using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �������� �� ���� ȯ���� �����ϴ� ��ũ��Ʈ. (�� ���� ���� ��)
/// </summary>
public class StageMaster : MonoBehaviour
{
    /// <summary>
    /// �ش� ������������ �÷��̾ ��� ������ ���� ����Ʈ
    /// </summary>
    [SerializeField]
    private List<UnitData> playerUnitList;

    /// <summary>
    /// �÷��̾ ��� ������ ���� ������ ī��Ʈ
    /// </summary>
    [SerializeField]
    private int playerUseUnitSlotCount;

    /// <summary>
    /// �÷��̾ ��� ������ ���� ������ ����
    /// </summary>
    [SerializeField]
    private int playerUseUnitSlotRange;

    /// <summary>
    /// �ش� ������������ ������ �����ϴ� ���� ����Ʈ
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
