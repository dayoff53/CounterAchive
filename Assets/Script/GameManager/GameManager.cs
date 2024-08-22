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
    SkillSelect,
    SkillPlay,

    UnitSelect,
    UnitSet
}

public partial class GameManager : Singleton<GameManager>
{
    # region UnitSlotVariable
    [Header("UnitSlot Data")]
    [Tooltip("���� ���� ����Ʈ")]
    public List<UnitSlotController> unitSlots;

    [SerializeField]
    [Tooltip("�� ������ ���� ��ġ�� ������ ��ųʸ�")]
    private SerializableDictionary<GameObject, Vector3> originalPositions = new SerializableDictionary<GameObject, Vector3>();

    [SerializeField]
    private UnitSlotGroupController unitSlotGroupController;

    /// <summary>
    /// ������ �̵� �� ���¸� �Ǵ��ϴ� ��
    /// </summary>
    private bool isMoving = false;
    #endregion


    #region TurnVariable
    [Space(10)]
    [Header("Turn Data")]
    [Tooltip("�ൿ�� ������ ���� Dictionary")]
    private SerializableDictionary<UnitSlotController, float> actionPoints = new SerializableDictionary<UnitSlotController, float>();

    [Tooltip("���� ���� ����Ʈ")]

    public int playerUseUnitSlotCount;

    /// <summary>
    /// ���� ���� ��� ���� ������ ��ȣ
    /// </summary>
    public int currentTurnSlotNumber;

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
            switch (_currentPrograssState)
            {
                default:
                    break;
            }

            _currentPrograssState = value;
        }
    }
    #endregion


    #region SkillSlotVariable
    [Space(10)]
    [Header("SkillSlot Data")]
    [SerializeField]
    [Tooltip("��ų ���� ����Ʈ")]
    public List<SkillSlotUIController> skillSlot;

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

    [SerializeField]
    public TMP_Text nullSlotText;


    [Space(10)]
    [Header("Color Data")]
    public List<Color> unitStateColors;
    public ColorState unitStateColorsObject;


    private void Awake()
    {


        //SetGame();
    }


    /// <summary>
    /// ���� �ʱ� ������ �����մϴ�.
    /// </summary>
    public void StartGame()
    {
        currentPrograssState = ProgressState.UnitSelect;

        nullSlotText.text = $"NullSlot : {playerUseUnitSlotCount}";





        SetUnitSlot(unitSlotGroupController.unitSlots);

        unitStateColors = unitStateColorsObject.colorStates;

        SlotPosInit();
        ActionPointsInit();
        StartCoroutine(ActionPointAccumulation());
    }



    /// <summary>
    /// ���� ��ġ �ʱ�ȭ�� ����մϴ�. (����� �ʱ� �ٶ󺸴� ���⸸ ���)
    /// </summary>
    public void SetUnitSlot(List<UnitSlotController> setUnitSlots)
    {
        unitSlots = setUnitSlots;

        for (int i = setUnitSlots.Count - 1; setUnitSlots.Count - 6 < i; i--)
        {
            setUnitSlots[i].SetDirection(false);
        }
    }

    /// <summary>
    /// �� unitSlot�� actionPoints�� �ʱ�ȭ�մϴ�.
    /// </summary>
    private void ActionPointsInit()
    {
        currentPrograssState = ProgressState.Stay;
        foreach (var unit in unitSlots)
        {
            if (unit != null && unit != null)
            {
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
        unitSlots[skillTargetNum].currentHp -= damage;
    }

    /// <summary>
    /// ������ ��󿡰� �������� ������ �����մϴ�.
    /// </summary>
    /// <param name="damage">������ �������Դϴ�.</param>
    public void ExecuteAttack(int targetNum, float damage)
    {
        unitSlots[targetNum].currentHp -= damage;
    }

    /// <summary>
    /// ������ ���鿡�� �������� ������ �����մϴ�.
    /// </summary>
    /// <param name="damage">������ �������Դϴ�.</param>
    public void ExecuteAttack(List<int> targetNums, float damage)
    {
        for (int i = 0; i < targetNums.Count; i++)
        {
            unitSlots[i].currentHp -= damage;
        }
    }
}
