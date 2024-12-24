using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// 턴 종류 (대기 => 유닛 플레이 => 스킬 대상 선택 => 스킬 이펙트 재생)
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


    #region StageVariable
    [SerializeField]
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

    public StageClearState stageClearCondition;
    public int targetEnemyId; // KillTargetEnemy 조건일 경우 특정 적의 ID
    public int surviveTurnCount; // SurviveTurn 조건일 경우 생존해야 할 턴 수
    private int currentTurnCount; // 현재 진행된 턴 수
    #endregion

    #region Unit&SlotVariable

    [Header("UnitSlot Data")]
    [SerializeField]
    [Tooltip("유닛 슬롯 그룹(unitSlots)을 담당하는 스크립트")]
    public UnitSlotGroupController unitSlotGroupController;

    [Tooltip("유닛 슬롯 리스트")]
    public List<UnitSlotController> unitSlotList;

    [SerializeField]
    [Tooltip("각 슬롯의 원래 위치를 저장할 딕셔너리")]
    private SerializableDictionary<int, Vector3> originalPositions = new SerializableDictionary<int, Vector3>();


    [Header("UnitSelect Data")]
    [SerializeField]
    [Tooltip("유닛 배치 단계에서 선택된 유닛 스텟")]
    public UnitStatus currentSelectUnitState;

    [Tooltip("플레이어가 사용 가능한 유닛 슬롯의 카운트")]
    public int playerUseUnitSlotCount;

    [Tooltip("플레이어가 사용 가능한 유닛 슬롯의 범위")]
    public int playerUseUnitSlotRange;

    /// <summary>
    /// 유닛의 이동 중 상태를 판단하는 값
    /// </summary>
    private bool isUnitMoving = false;
    /// <summary>
    /// 유닛의 죽는 중 상태를 판단하는 값
    /// </summary>
    public bool isUnitDying = false;
    #endregion

    #region SkillSlotVariable
    [Space(10)]
    [Header("SkillSlot Data")]
    [SerializeField]
    [Tooltip("스킬 슬롯 리스트")]
    /// <summary>
    /// 스킬 데이터를 플레이어에게 보여주는 UI 슬롯 리스트
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

            if (targetUnitCardUI != null && skillTargetNum >= 0)
            {
                targetUnitCardUI.unitStatus.SetStatus(unitSlotList[skillTargetNum].unit);
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



    protected virtual void Start()
    {
        dataManager = DataManager.Instance;
        poolManager = PoolManager.Instance;
    }

    /// <summary>
    /// 게임 초기 설정을 진행하는 단계입니다.
    /// </summary>
    public void InitGame()
    {
        unitSlotGroupController.UnitSlotsInit();
        unitStateColors = unitStateColorsObject.colorStates;
        SlotPosInit();

        UnitSetGame();
    }

    /// <summary>
    /// 플레이어가 유닛을 배치하는 단계입니다.
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
                    unitSlotList[i].unitTeam = 1;
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
                    unitSlotList[i].unitTeam = 0;
                }

                unitSlotList[i].slotGround.SetSlotGroundState(SlotGroundState.Default);
            }

            StartGame();
        }
    }

    /// <summary>
    /// 메인 게임을 실시하는 단계입니다.
    /// </summary>
    public void StartGame()
    {
        ActionPointsInit();
        StartCoroutine(ActionPointAccumulation());
    }

    /// <summary>
    /// 각 unitSlot의 actionPoints를 초기화합니다.
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
                    actionPoints.Add(unit, unit.currentAp); // 키가 없으면 추가
                }
            }
        }
    }


    private void UpdateStageClearCondition()
    {
        switch (stageClearCondition)
        {
            case StageClearState.KillAllEnemy:
                if (unitSlotList.All(slot => slot.isNull || slot.unitTeam != 2)) // 적이 전멸했는지 확인
                {
                    StageClear();
                }
                break;

            case StageClearState.KillTargetEnemy:
                if (unitSlotList.Any(slot => slot.unit.unitNumber == targetEnemyId && slot.isNull))
                {
                    StageClear();
                }
                break;

            case StageClearState.SurviveTurn:
                if (currentTurnCount >= surviveTurnCount)
                {
                    StageClear();
                }
                break;

            default:
                break;
        }
    }
    private void StageClear()
    {
        Debug.Log("Stage Cleared!");
        // 클리어 연출 및 다음 단계 처리 로직 추가
    }

}
