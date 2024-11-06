using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �������� �� ���� ȯ���� �����ϴ� ��ũ��Ʈ. ���� �Ʊ��� ��ġ, ���������� �к� �� �¸� ����, gameManager�� �������� ���� ������ ������ ��, ���������� �ҷ��ö� ���� �켱������ �۵��Ͽ� �������� ȯ���� �����ϴ� ��ũ��Ʈ�̴�.
/// </summary>
public class StageMaster : MonoBehaviour
{
    [Header("�������� �� ���� ȯ���� �����ϴ� ��ũ��Ʈ ���������� �ҷ��ö� ���� �켱������ �۵��ϴ� ��ũ��Ʈ�̴�. (�� ���� ���� ��)")]

    [SerializeField]
    private StageManager gameManager;

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
    /// �ش� ������������ �÷��̾ ��� �����ϵ��� ��ġ�Ǿ� �ִ� ���� ����Ʈ
    /// </summary>
    [SerializeField]
    private List<UnitState> playerUnitList;

    /// <summary>
    /// �ش� ������������ ������ �����ϴ� ���� ����Ʈ
    /// </summary>
    [SerializeField]
    private List<UnitState> enemyUnitList;

    /// <summary>
    /// ��ų �����͸� �÷��̾�� �����ִ� UI ���� ����Ʈ
    /// </summary>
    [SerializeField]
    private List<SkillSlotUIController> skillSlotList;


    /// <summary>
    /// �� ���ӿ��� ������ �����ϴ� ������ �׷�
    /// </summary>
    [SerializeField]
    private UnitSlotGroupController unitSlotGroupController;

    /// <summary>
    /// �÷��̾ ��ġ ������ ������ �����ϴ� ������ �׷�
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
    /// ���������� ��ġ�� ���ֵ��� ��ġ�մϴ�.
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
