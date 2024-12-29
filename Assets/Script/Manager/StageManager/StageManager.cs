using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// 턴 순서 (대기 => 유닛 플레이 턴 => 스킬 타겟 선택 => 스킬 이펙트 실행 (게임 실행 전 유닛 선택 턴, 선택 후 유닛 베치 턴))
/// </summary>
public enum ProgressState
{
    Stay,
    UnitPlay,

    SkillTargetSearch,
    SkillPlay,

    UnitSelect
}

public partial class StageManager : Singleton<StageManager>
{
    //[Header("------------------- Manager -------------------")]
    private DataManager dataManager;
    private PoolManager poolManager;
    private CameraManager cameraManager;

    #region StageVariable
    [Header("------------------- Stage -------------------")]
    public StageClearState stageClearCondition;

    public int targetEnemyId; // KillTargetEnemy 조건일 경우 특정 적의 ID
    public GameObject lastEnemyDeathObject; // 최근 사망한 마지막 적
    public int surviveTurnCount; // SurviveTurn 조건일 경우 생존해야 할 턴 수
    private int currentTurnCount; // 현재 진행된 턴 수
    
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
                    UIManager.Instance.SwitchUIMode(false);
                    break;

                default:
                    UIManager.Instance.SwitchUIMode(true);
                    break;
            }
            Debug.Log($"{currentPrograssState} => {value}");

            _currentPrograssState = value;
        }
    }
    #endregion

    #region Unit&SlotVariable
    [Space(20)]
    [Header("------------------- Unit & Slot -------------------")]
    [Header("UnitSlot 변수")]
    [SerializeField]
    [Tooltip("유닛 슬롯 그룹(unitSlots)을 관리하는 스크립트")]
    public UnitSlotGroupController unitSlotGroupController;

    [Tooltip("유닛 슬롯 리스트")]
    public List<UnitSlotController> unitSlotList;

    [SerializeField]
    [Tooltip("각 슬롯의 원래 위치를 저장할 딕셔너리")]
    private SerializableDictionary<int, Vector3> slotOriginalPositions = new SerializableDictionary<int, Vector3>();


    [Header("UnitSelect Data")]
    [SerializeField]
    [Tooltip("유닛 배치 단계에서 선택된 유닛 상태")]
    public UnitStatus currentSelectUnitState;

    [Tooltip("플레이어가 사용 가능한 유닛 슬롯의 카운트")]
    public int playerUseUnitSlotCount;

    [Tooltip("플레이어가 사용 가능한 유닛 슬롯의 범위")]
    public int playerUseUnitSlotRange;

    /// <summary>
    /// 유닛이 죽는 중 상태를 판단하는 값
    /// </summary>
    public bool isUnitDying = false;
    #endregion
    



    protected virtual void Start()
    {
        dataManager = DataManager.Instance;
        poolManager = PoolManager.Instance;
        cameraManager = CameraManager.Instance;
    }

    /// <summary>
    /// 게임 초기 설정을 진행하는 단계입니다. StageMaster에서 변수 할당 후 호출합니다.
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
            // 플레이어가 사용 가능한 유닛 슬롯이 있을 경우 유닛 배치 단계로 진행
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
            

            UIManager.Instance.UpdateRemainingSlots(playerUseUnitSlotCount);
        } else
        {
            // 플레이어가 사용 가능한 유닛 슬롯이 없을 경우 비어있는 유닛 슬롯의 Team을 0으로 설정하고 게임 시작

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
    /// 게임 시작을 실시하는 단계입니다.
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

    /// <summary>
    /// 스테이지 클리어 조건을 업데이트하는 함수입니다. (턴을 넘기는 기능도 겸합니다.)
    /// </summary>
                bool noEnemiesLeft = true;
    private void UpdateStageClearCondition()
    {
        switch (stageClearCondition)
        {
            case StageClearState.KillAllEnemy:
                noEnemiesLeft = true;
                foreach(var slot in unitSlotList) {
                    if(slot.unitTeam == 2) {
                        noEnemiesLeft = false;
                    }
                }
                if(noEnemiesLeft) // 적이 하나도 없다면 클리어 조건 달성
                {
                    Debug.Log("noEnemiesLeft" + noEnemiesLeft);
                    cameraManager.ZoomToTarget(lastEnemyDeathObject.transform, 3.5f, 0.5f);
                    Time.timeScale = 0.5f;
                    Invoke(nameof(KillAllEnemyClear), 2.1f);
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
        UnitSlotListInit();
            currentPrograssState = ProgressState.Stay;
    }

    /// <summary>
    /// 스테이지 클리어 조건을 달성했을 때 호출되는 함수입니다.
    /// </summary>
    private void StageClear()
    {
        Debug.Log("Stage Cleared!");
        // 클리어 조건 달성 시 다음 단계 처리 로직 추가
    }

    private void KillAllEnemyClear()
    {
        Time.timeScale = 1.0f;
        cameraManager.ResetCamera(0.5f);
        StageClear();
    }
}
