using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum StageClearState
{
    KillAllEnemy,
    KillTargetEnemy,
    SurviveTurn
}

/// <summary>
/// �������� �� ���� ȯ���� �����ϴ� ��ũ��Ʈ. ���� �Ʊ��� ��ġ, ���������� �й� �� �¸� ����, gameManager�� �������� ���� ������ ������ ��, 
/// ���������� �ҷ��� �� ���� �켱������ �۵��Ͽ� �������� ȯ���� �����ϴ� ��ũ��Ʈ�̴�.
/// </summary>
public class StageMaster : MonoBehaviour
{
    [Header("�������� �� ���� ȯ���� �����ϴ� ��ũ��Ʈ ���������� �ҷ��ö� ���� �켱������ �۵��ϴ� ��ũ��Ʈ�̴�. (�� ���� ���� ��)")]
    [Header("Stage Manager Reference")]
    [SerializeField]
    private StageManager stageManager;

    [Header("Stage Clear Condition")]
    public StageClearState stageClearCondition;
    [Tooltip("KillTargetEnemy ������ ��� Ư�� ���� ID")]
    public int targetEnemyId;
    [Tooltip("SurviveTurn ������ ��� �����ؾ� �� �� ��")]
    public int surviveTurnCount;

    [Space(20)]
    [Header("Player Unit Settings")]
    [Tooltip("�÷��̾ ��� ������ ���� ������ ī��Ʈ")]
    [SerializeField]
    private int playerUseUnitSlotCount;
    [Tooltip("�÷��̾ ��� ������ ���� ������ ����")]
    [SerializeField]
    private int playerUseUnitSlotRange;
    [Tooltip("�ش� ������������ �÷��̾ ��� �����ϵ��� ��ġ�Ǿ� �ִ� ���� ����Ʈ")]
    [SerializeField]
    private List<UnitStatus> playerUnitList;

    [Space(20)]
    [Header("Enemy Unit Settings")]
    [Tooltip("�ش� ������������ ������ �����ϴ� ���� ����Ʈ")]
    [SerializeField]
    private List<UnitStatus> enemyUnitList;

    [Space(20)]
    [Header("UI Elements")]
    [Tooltip("���� �÷��� ���� ������ �����ϴ� UI")]
    [SerializeField]
    public GameObject unitSet_UI;
    [Tooltip("�����ִ� ���� ������ ���� ���� ������ �����ִ� �ؽ�Ʈ")]
    [SerializeField]
    public TMP_Text remainingSetUnitSlotText;

    [Space(5)]
    [Tooltip("������ �ַ� �÷����ϴ� UI")]
    [SerializeField]
    public GameObject play_UI;
    [Tooltip("���� ���� �÷��̾��� ���� ī�� UI")]
    [SerializeField]
    public UnitCard turnUnitCardUI;
    [Tooltip("���� Ÿ���� �� ���� ī�� UI")]
    [SerializeField]
    public UnitCard targetUnitCardUI;
    [Tooltip("Ÿ�� ����ī�带 ����Ű�� ȭ��ǥ")]
    [SerializeField]
    public GameObject unitCardTargetArrow;
    [Tooltip("��ų ���߷� �ؽ�Ʈ")]
    [SerializeField]
    public TMP_Text skillAccuracyText;
    [Tooltip("���� ���� ������ ǥ���ϴ� ��Ŀ")]
    [SerializeField]
    public GameObject turnUnitMarker;
    [Tooltip("Ÿ�� ������ ǥ���ϴ� ��Ŀ")]
    [SerializeField]
    public GameObject targetUnitMarker;
    [Tooltip("�������� ���� �ϴ��� �޴� UI")]
    [SerializeField]
    public StageMenuController stageMenuController;

    [Space(20)]
    [Header("Skill Settings")]
    [Tooltip("��ų �����͸� �÷��̾�� �����ִ� UI ���� ����Ʈ")]
    [SerializeField]
    private List<SkillSlotUIController> skillSlotList;

    [Space(20)]
    [Header("Unit Slot Settings")]
    [Tooltip("�� ���ӿ��� ������ �����ϴ� ������ �׷�")]
    [SerializeField]
    private UnitSlotGroupController unitSlotGroupController;
    [Tooltip("�÷��̾ ��ġ ������ ������ �����ϴ� ������ �׷�")]
    [SerializeField]
    private UnitSelectSlotGroupController unitSelectSlotsGroupController;


    private void InitUIObject()
    {
        stageManager.turnUnitCardUI = turnUnitCardUI;
        stageManager.targetUnitCardUI = targetUnitCardUI;
        stageManager.skillAccuracyText = skillAccuracyText;
        stageManager.turnUnitMarker = turnUnitMarker;
        stageManager.targetUnitMarker = targetUnitMarker;
        stageManager.play_UI = play_UI;
        stageManager.unitSet_UI = unitSet_UI;
        stageManager.remainingSetUnitSlotText = remainingSetUnitSlotText;
        stageManager.stageMenuController = stageMenuController;
    }

    private void Start()
    {
        stageManager = StageManager.Instance;

        InitUIObject();

        unitSelectSlotsGroupController.InitUnitSelectSlot();

        PlaceUnitSlot();

        stageManager.stageClearCondition = stageClearCondition;
        stageManager.targetEnemyId = targetEnemyId;
        stageManager.surviveTurnCount = surviveTurnCount;

        stageManager.playerUseUnitSlotCount = playerUseUnitSlotCount;
        stageManager.playerUseUnitSlotRange = playerUseUnitSlotRange;

        stageManager.skillSlotList = skillSlotList;
        Debug.Log($"skillSlotList.Count {skillSlotList.Count}\n skillSlotList[0] : {skillSlotList[0]}");
        Debug.Log($"gameManager.skillSlotList.Count {stageManager.skillSlotList.Count}\n gameManager.skillSlotList[0] : {stageManager.skillSlotList[0]}");

         
        stageManager.InitGame();
    }

    /// <summary>
    /// ���������� ��ġ�� ���ֵ��� ��ġ�մϴ�.
    /// </summary>
    private void PlaceUnitSlot()
    {
        int startNum = 0;
        for (int i = 0; i < playerUnitList.Count; i++)
        {
            unitSlotGroupController.unitSlots[startNum + i].SetUnit(playerUnitList[i], 1);
        }

        int endNum = unitSlotGroupController.unitSlots.Count - 1;
        for (int i = 0; i < enemyUnitList.Count; i++)
        {
            if(enemyUnitList[i].unitName != "Null")
            {
                enemyUnitList[i].SetStatus(enemyUnitList[i].defaultUnitData);
            }
            unitSlotGroupController.unitSlots[endNum - i].SetUnit(enemyUnitList[i], 2);
            unitSlotGroupController.unitSlots[endNum - i].unit.SetDirection(true);
        }
    }
}
