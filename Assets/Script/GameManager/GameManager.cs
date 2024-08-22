using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 턴 종류 (대기 => 유닛 플레이 => 스킬 대상 선택 => 스킬 이펙트 재생)
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
    [Tooltip("유닛 슬롯 리스트")]
    public List<UnitSlotController> unitSlots;

    [SerializeField]
    [Tooltip("각 슬롯의 원래 위치를 저장할 딕셔너리")]
    private SerializableDictionary<GameObject, Vector3> originalPositions = new SerializableDictionary<GameObject, Vector3>();

    [SerializeField]
    private UnitSlotGroupController unitSlotGroupController;

    /// <summary>
    /// 유닛의 이동 중 상태를 판단하는 값
    /// </summary>
    private bool isMoving = false;
    #endregion


    #region TurnVariable
    [Space(10)]
    [Header("Turn Data")]
    [Tooltip("행동력 누적을 위한 Dictionary")]
    private SerializableDictionary<UnitSlotController, float> actionPoints = new SerializableDictionary<UnitSlotController, float>();

    [Tooltip("유닛 슬롯 리스트")]

    public int playerUseUnitSlotCount;

    /// <summary>
    /// 현재 턴을 행사 중인 슬롯의 번호
    /// </summary>
    public int currentTurnSlotNumber;

    private ProgressState _currentPrograssState = ProgressState.UnitSelect;
    /// <summary>
    /// 현재 게임 진행상황
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
    [Tooltip("스킬 슬롯 리스트")]
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
    [Tooltip("현재 턴을 지닌 유닛의 얼굴 아이콘")]
    public Image currentTurnSlotIcon;

    [SerializeField]
    [Tooltip("현재 턴을 지닌 유닛의 이름 텍스트")]
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
    /// 게임 초기 설정을 실행합니다.
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
    /// 슬롯 위치 초기화를 담당합니다. (현재는 초기 바라보는 방향만 담당)
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
    /// 각 unitSlot의 actionPoints를 초기화합니다.
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
                    actionPoints.Add(unit, unit.currentActionPoint); // 키가 없으면 추가
                }
            }
        }
    }


    /// <summary>
    /// 현 목표에게 공격을 수행합니다.
    /// </summary>
    /// <param name="damage">적용할 데미지입니다.</param>
    public void ExecuteAttack(float damage)
    {
        unitSlots[skillTargetNum].currentHp -= damage;
    }

    /// <summary>
    /// 지정된 대상에게 데미지로 공격을 수행합니다.
    /// </summary>
    /// <param name="damage">적용할 데미지입니다.</param>
    public void ExecuteAttack(int targetNum, float damage)
    {
        unitSlots[targetNum].currentHp -= damage;
    }

    /// <summary>
    /// 지정된 대상들에게 데미지로 공격을 수행합니다.
    /// </summary>
    /// <param name="damage">적용할 데미지입니다.</param>
    public void ExecuteAttack(List<int> targetNums, float damage)
    {
        for (int i = 0; i < targetNums.Count; i++)
        {
            unitSlots[i].currentHp -= damage;
        }
    }
}
