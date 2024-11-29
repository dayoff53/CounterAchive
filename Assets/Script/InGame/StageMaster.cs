using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// �������� �� ���� ȯ���� �����ϴ� ��ũ��Ʈ. ���� �Ʊ��� ��ġ, ���������� �к� �� �¸� ����, gameManager�� �������� ���� ������ ������ ��, ���������� �ҷ��ö� ���� �켱������ �۵��Ͽ� �������� ȯ���� �����ϴ� ��ũ��Ʈ�̴�.
/// </summary>
public class StageMaster : MonoBehaviour
{
    [Header("�������� �� ���� ȯ���� �����ϴ� ��ũ��Ʈ ���������� �ҷ��ö� ���� �켱������ �۵��ϴ� ��ũ��Ʈ�̴�. (�� ���� ���� ��)")]

    [SerializeField]
    private StageManager stageManager;

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
    private List<UnitStatus> playerUnitList;

    /// <summary>
    /// �ش� ������������ ������ �����ϴ� ���� ����Ʈ
    /// </summary>
    [SerializeField]
    private List<UnitStatus> enemyUnitList;

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



    [Tooltip("���� ���� ���� �÷��̾��� ���� ī�� UI")]
    [SerializeField]
    public UnitCardController currentPlayerUnitCardUI;

    [Tooltip("���� Ÿ���� �� ���� ī�� UI")]
    [SerializeField]
    public UnitCardController currentTargetUnitCardUI;

    [Tooltip("Ÿ�� ����ī�带 ����Ű�� ȭ��ǥ")]
    [SerializeField]
    public Image unitCardTargetArrow;

    [Tooltip("������ �ַ� �÷����ϴ� UI")]
    [SerializeField]
    public GameObject play_UI;

    [Tooltip("���� �÷��� ���� ������ �����ϴ� UI")]
    [SerializeField]
    public GameObject unitSet_UI;

    [Tooltip("�����ִ� ���� ������ ���� ���� ������ �����ִ� �ؽ�Ʈ")]
    [SerializeField]
    public TMP_Text remainingSetUnitSlotText;


    private void InitUIObject()
    {
        stageManager.currentPlayerUnitCardUI = currentPlayerUnitCardUI;
        stageManager.currentTargetUnitCardUI = currentTargetUnitCardUI;
        stageManager.play_UI = play_UI;
        stageManager.unitSet_UI = unitSet_UI;
        stageManager.remainingSetUnitSlotText = remainingSetUnitSlotText;
    }

    private void Start()
    {
        stageManager = StageManager.Instance;

        InitUIObject();

        unitSelectSlotsGroupController.InitUnitSelectSlot();

        PlaceUnitSlot();

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
            unitSlotGroupController.unitSlots[endNum - i].SetUnit(enemyUnitList[i], 2);
        }
    }
}
