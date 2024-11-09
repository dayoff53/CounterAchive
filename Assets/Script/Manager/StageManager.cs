using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// �� ���� (��� => ���� �÷��� => ��ų ��� ���� => ��ų ����Ʈ ���)
/// </summary>
public enum ProgressState
{
    Stay,
    UnitPlay,

    SkillTargetSearch,
    SkillPlay,

    UnitSelect,
    UnitSet
}

public partial class StageManager : Singleton<StageManager>
{
    [SerializeField]
    private ProgressState _currentPrograssState = ProgressState.UnitSelect;
    /// <summary>
    /// ���� ���� �����Ȳ
    /// </summary>
    public ProgressState currentPrograssState
    {
        get
        { return _currentPrograssState; }
        set
        {
            switch (value)
            {
                case ProgressState.UnitSelect:
                    play_UI.SetActive(false);
                    unitSet_UI.SetActive(true);
                    break;

                default:
                    unitSet_UI.SetActive(false);
                    play_UI.SetActive(true);
                    break;
            }
            Debug.Log($"{currentPrograssState} => {value}");

            _currentPrograssState = value;
        }
    }


    #region Unit&SlotVariable

    [Header("UnitSlot Data")]

    [SerializeField]
    [Tooltip("���� ���� �׷�(unitSlots)�� ����ϴ� ��ũ��Ʈ")]
    private UnitSlotGroupController unitSlotsController;

    [SerializeField]
    [Tooltip("���� ���õ� ���� ����")]
    public UnitState currentSelectUnitState;

    [Tooltip("�÷��̾ ��� ������ ���� ������ ī��Ʈ")]
    public int playerUseUnitSlotCount;

    [Tooltip("�÷��̾ ��� ������ ���� ������ ����")]
    public int playerUseUnitSlotRange;

    [Tooltip("���� ���� ����Ʈ")]
    public List<UnitSlotController> unitSlotList;

    [SerializeField]
    [Tooltip("�� ������ ���� ��ġ�� ������ ��ųʸ�")]
    private SerializableDictionary<GameObject, Vector3> originalPositions = new SerializableDictionary<GameObject, Vector3>();

    /// <summary>
    /// ������ �̵� �� ���¸� �Ǵ��ϴ� ��
    /// </summary>
    private bool isMoving = false;
    #endregion

    #region TurnVariable
    [Space(10)]
    [Header("Turn Data")]
    [Tooltip("�ൿ�� ������ ���� Dictionary")]
    private SerializableDictionary<UnitController, float> actionPoints = new SerializableDictionary<UnitController, float>();

    /// <summary>
    /// ���� ���� ��� ���� ������ ��ȣ
    /// </summary>
    public int currentTurnSlotNumber;

    #endregion

    #region SkillSlotVariable
    [Space(10)]
    [Header("SkillSlot Data")]
    [SerializeField]
    [Tooltip("��ų ���� ����Ʈ")]
    /// <summary>
    /// ��ų �����͸� �÷��̾�� �����ִ� UI ���� ����Ʈ
    /// </summary>
    public List<SkillSlotUIController> skillSlotList;

    public int skillTargetNum;

    public SkillSlotUIController currentSkillSlot;
    private UnitSlotController targetUnitSlot;
    #endregion

    #region CostVariable
    [Space(10)]
    [Header("Cost Data")]
    private float _cost;
    public float cost 
    {
        get { return _cost; }
        set 
        {
            if(_cost != value)
            {
                _cost = value;
                costBar.fillAmount = _cost / 10;
                costGauge.fillAmount = _cost - Mathf.Floor(_cost);
                if (_cost > 10)
                {
                    costGauge.fillAmount = 1;
                }
                costText.text = Mathf.FloorToInt(_cost).ToString();
            }
        }
    }

    [SerializeField]
    private Image costGauge;
    [SerializeField]
    private TMP_Text costText;
    [SerializeField]
    private Image costBar;
    #endregion


    [Space(10)]
    [Header("UI Object")]
    [SerializeField]
    [Tooltip("���� ���� ���� ������ �� ������")]
    public Image currentTurnSlotIcon;

    [SerializeField]
    [Tooltip("���� ���� ���� ������ �̸� �ؽ�Ʈ")]
    public TMP_Text currentTurnName;

    [SerializeField]
    public GameObject play_UI;
    [SerializeField]
    public GameObject unitSet_UI;

    /// <summary>
    /// �����ִ� ���� ������ ���� ����
    /// </summary>
    [SerializeField]
    public TMP_Text remainingSetUnitSlotText;


    [Space(10)]
    [Header("Color Data")]
    public List<Color> unitStateColors;
    public ColorState unitStateColorsObject;


    /// <summary>
    /// ���� �ʱ� ������ �����ϴ� �ܰ��Դϴ�.
    /// </summary>
    public void SetGame()
    {
        unitSlotsController.UnitSlotsInit();
        unitStateColors = unitStateColorsObject.colorStates;
        SlotPosInit();

        UnitSetGame();
    }

    /// <summary>
    /// �÷��̾ ������ ��ġ�ϴ� �ܰ��Դϴ�.
    /// </summary>
    public void UnitSetGame()
    {
        if (playerUseUnitSlotCount > 0)
        {
            currentPrograssState = ProgressState.UnitSelect;

            for(int i = 0; (i < unitSlotList.Count); i++)
            {
                if (i < playerUseUnitSlotRange && unitSlotList[i].isNull == true)
                {
                    unitSlotList[i].unit.unitTeam = 1;
                    unitSlotList[i].slotGround.SetSlotGroundState(SlotGroundState.Target);
                } else
                {
                    unitSlotList[i].slotGround.SetSlotGroundState(SlotGroundState.Default);
                }
            }
            

            remainingSetUnitSlotText.text = $"RemainingUnitSlot : {playerUseUnitSlotCount}";
        } else
        {

            for (int i = 0; (i < unitSlotList.Count); i++)
            {
                if (unitSlotList[i].isNull == true)
                {
                    unitSlotList[i].unit.unitTeam = 0;
                }

                unitSlotList[i].slotGround.SetSlotGroundState(SlotGroundState.Default);
            }

            StartGame();
        }
    }

    /// <summary>
    /// ���� ������ �ǽ��ϴ� �ܰ��Դϴ�.
    /// </summary>
    public void StartGame()
    {
        ActionPointsInit();
        StartCoroutine(ActionPointAccumulation());
    }

    /// <summary>
    /// �� unitSlot�� actionPoints�� �ʱ�ȭ�մϴ�.
    /// </summary>
    private void ActionPointsInit()
    {
        currentPrograssState = ProgressState.Stay;
        foreach (var unitSlot in unitSlotList)
        {
            if (unitSlot != null && unitSlot != null)
            {
                UnitController unit = unitSlot.unit;
                if (!actionPoints.ContainsKey(unit))
                {
                    actionPoints.Add(unit, unit.currentActionPoint); // Ű�� ������ �߰�
                }
            }
        }
    }


    /// <summary>
    /// �� ��ǥ���� ������ �����մϴ�.
    /// </summary>
    /// <param name="damage">������ �������Դϴ�.</param>
    public void ExecuteAttack(float damage)
    {
        unitSlotList[skillTargetNum].unit.currentHp -= damage;
    }

    /// <summary>
    /// ������ ��󿡰� �������� ������ �����մϴ�.
    /// </summary>
    /// <param name="damage">������ �������Դϴ�.</param>
    public void ExecuteAttack(int targetNum, float damage)
    {
        unitSlotList[targetNum].unit.currentHp -= damage;
    }

    /// <summary>
    /// ������ ���鿡�� �������� ������ �����մϴ�.
    /// </summary>
    /// <param name="damage">������ �������Դϴ�.</param>
    public void ExecuteAttack(List<int> targetNums, float damage)
    {
        for (int i = 0; i < targetNums.Count; i++)
        {
            unitSlotList[i].unit.currentHp -= damage;
        }
    }
}
