using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor.EditorTools;

/// <summary>
/// 턴 순서 (대기 => 유닛 플레이 턴 => 스킬 타겟 선택 => 스킬 이펙트 실행 (게임 실행 전 유닛 선택 턴, 선택 후 유닛 베치 턴))
/// </summary>
public enum ProgressState
{
    Stay,
    UnitPlay,

    SkillTargetSearch,
    SkillPlay,

    UnitSelect,
    GameEnd
}
public enum StageClearState
{
    KillAllEnemy,
    KillTargetEnemy,
    SurviveTurn
}

[DetailedInfoBox("전투 스테이지를 관리하는 매니저", "전투 스테이지를 관리하는 매니저로 \n해당 스크립트가 존제하는 스테이지의 기본 정보를 가지고 있으며, 스테이지와 플레이어의 정보를 토대로 전투 시스템의 전반적인 흐름을 제어하는 스크립트입니다.")]
public partial class BattleStageMaster : MonoBehaviour
{
    //[Header("------------------- Manager -------------------")]
    private DataManager dataManager;
    private PoolManager poolManager;
    private CameraManager cameraManager;

    #region StageVariable
    [Title("Stage")]
    public StageClearState stageClearCondition;

    public int targetEnemyId; // KillTargetEnemy 조건일 경우 특정 적의 ID
    public GameObject lastEnemyDeathObject; // 최근 사망한 마지막 적
    public int surviveTurnCount; // SurviveTurn 조건일 경우 생존해야 할 턴 수
    private int currentTurnCount; // 현재 진행된 턴 수
    
    [Space(5)]
    [ReadOnly]
    [DetailedInfoBox("현재 게임 진행상황", "현재 게임 진행상황 \n\n대기 => \n유닛 플레이 턴 => \n스킬 타겟 선택 => \n스킬 이펙트 실행 (게임 실행 전 유닛 선택 턴, 선택 후 유닛 베치 턴)")]
    [SerializeField]
    private ProgressState _currentPrograssState = ProgressState.UnitSelect;

    /// <summary>
    /// 현재 게임 진행상황 (게임 UI도 여기서 변경됨)
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
                    SwitchUIMode(false);
                    break;

                default:
                    SwitchUIMode(true);
                    break;
            }
            Debug.Log($"{currentPrograssState} => {value}");

            _currentPrograssState = value;
        }
    }
    
    [Space(5)]
    [Tooltip("해당 스테이지에서 플레이어가 사용 가능하도록 사전에 배치되어 있는 유닛 리스트")]
    [SerializeField]
    private List<UnitStatus> playerUnitList;

    [Tooltip("해당 스테이지에서 적으로 등장하는 유닛 리스트")]
    [SerializeField]
    private List<UnitStatus> enemyUnitList;
    #endregion

    #region Unit&SlotVariable
    [Space(20)]
    [Title("Unit & Slot")]
    [SerializeField]
    [Header("UnitSlot 변수")]
    [Tooltip("유닛 슬롯 그룹(unitSlots)을 관리하는 스크립트")]
    public UnitSlotGroupController unitSlotGroupController;

    [Tooltip("유닛 슬롯 리스트")]
    public List<UnitSlotController> unitSlotList;

    [SerializeField]
    [Tooltip("각 슬롯의 원래 위치를 저장할 딕셔너리")]
    private SerializableDictionary<int, Vector3> slotOriginalPositions = new SerializableDictionary<int, Vector3>();


    [Header("UnitSelect Data")]
    [SerializeField]
    [Tooltip("플레이어가 배치 가능한 유닛이 존재하는 슬롯을 총괄하는 스크립트")]
    private UnitSelectSlotGroupController unitSelectSlotGroupController;

    [SerializeField]
    [Tooltip("유닛 배치 단계에서 선택된 유닛 상태")]
    public UnitStatus currentSelectUnitState;

    [Tooltip("플레이어가 사용 가능한 유닛 슬롯의 카운트")]
    public int playerUseUnitSlotCount;

    [Tooltip("플레이어가 사용 가능한 유닛 슬롯의 범위")]
    public int playerUseUnitSlotRange;
    
    [SerializeField]
    [Tooltip("유닛이 죽는 중 상태를 판단하는 값")]
    public bool isUnitDying = false;
    #endregion
    



    protected virtual void Start()
    {
        dataManager = DataManager.Instance;
        poolManager = PoolManager.Instance;
        cameraManager = CameraManager.Instance;
        dataManager.battleStageMaster = this;

        InitGame();
    }

    /// <summary>
    /// 게임 초기 설정을 진행하는 단계입니다.
    /// </summary>
    public void InitGame()
    {
        poolManager.Clear();
        cameraManager.Init();
        unitSlotGroupController.Init(this);
        unitSelectSlotGroupController.Init(this);
        unitStateColors = dataManager.unitColorStateObject.colorStates;
        
        SlotPosInit();
        PlaceUnitSlot();
        UnitSetGame();
    }

    /// <summary>
    /// 스테이지에 위치한 유닛들을 배치합니다.
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
                enemyUnitList[i].SetStatus(enemyUnitList[i].unitData);
            }
            unitSlotGroupController.unitSlots[endNum - i].SetUnit(enemyUnitList[i], 2);
            unitSlotGroupController.unitSlots[endNum - i].unit.isFlipX = true;
        }
    }

    /// <summary>
    /// 플레이어가 유닛을 배치하는 단계입니다. 배치가 종료되면 게임을 시작합니다.
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
            
            unitSetUIController.UpdateRemainingSlots(playerUseUnitSlotCount);
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
        stageMenuController.Init(this);
        stageMenuController.StageMenuRefresher();
        
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
    /// 스테이지 클리어 조건을 업데이트하는 함수입니다. (턴을 넘기는 기능도 합니다.)
    /// </summary>
    public bool UpdateStageClearCondition()
    {
        if(currentPrograssState != ProgressState.GameEnd)
        {
            currentPrograssState = ProgressState.GameEnd;
            switch (stageClearCondition)
            {
                case StageClearState.KillAllEnemy:
                bool noEnemiesLeft = true;
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

                    Invoke(nameof(StageClear), 2.1f);
                    return false;
                }
                break;

            case StageClearState.KillTargetEnemy:
                if (unitSlotList.Any(slot => slot.unit.unitNumber == targetEnemyId && slot.isNull))
                {
                    StageClear();
                    return false;
                }
                break;

            case StageClearState.SurviveTurn:
                if (currentTurnCount >= surviveTurnCount)
                {
                    StageClear();
                    return false;
                }
                break;
            default:
                    break;
            }
        }
            AllUnitBaseUpdate();
            currentPrograssState = ProgressState.Stay;
            return true;
    }

    /// <summary>
    /// 스테이지 클리어 조건을 달성했을 때 호출되는 함수입니다.
    /// </summary>
    private void StageClear()
    {
        Time.timeScale = 1.0f;
        cameraManager.ResetCamera(0.5f);
        Debug.Log("Stage Cleared!");
        
        Debug.Log("SetActive : true");
        win_UI.SetActive(true);
        winUIController.WinUIActive();
    }
}
