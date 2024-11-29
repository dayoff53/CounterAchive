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
    private DataManager dataManager;
    private PoolManager poolManager;

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
    public UnitStatus currentSelectUnitState;

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
    private SerializableDictionary<UnitBase, float> actionPoints = new SerializableDictionary<UnitBase, float>();

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

    private int _skillTargetNum;

    public int skillTargetNum
    {
        get
        {
            return _skillTargetNum;
        }
        set
        {
            _skillTargetNum = value;

            if (currentTargetUnitCardUI != null)
            {
                currentTargetUnitCardUI.unitStatus.SetStatus(unitSlotList[skillTargetNum].unit);
            }
        }
    }

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

    #region UIVariable
    [Space(10)]
    [Header("UI Object")]
    public UnitCardController currentPlayerUnitCardUI;
    public UnitCardController currentTargetUnitCardUI;
    public GameObject play_UI;
    public GameObject unitSet_UI;
    public TMP_Text remainingSetUnitSlotText;


    [Space(10)]
    [Header("Color Data")]
    public List<Color> unitStateColors;
    public ColorState unitStateColorsObject;
    #endregion

    protected virtual void Start()
    {
        dataManager = DataManager.Instance;
        poolManager = PoolManager.Instance;
    }


    /// <summary>
    /// ���� �ʱ� ������ �����ϴ� �ܰ��Դϴ�.
    /// </summary>
    public void InitGame()
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
                UnitBase unit = unitSlot.unit;
                if (!actionPoints.ContainsKey(unit))
                {
                    actionPoints.Add(unit, unit.currentAP); // Ű�� ������ �߰�
                }
            }
        }
    }

}
