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
/// 스테이지 별 게임 환경을 설정하는 스크립트. 적과 아군의 배치, 스테이지의 패배 및 승리 조건, gameManager에 스테이지 관련 데이터 보내기 등, 
/// 스테이지를 불러올 때 가장 우선적으로 작동하여 스테이지 환경을 구성하는 스크립트이다.
/// </summary>
public class StageMaster : MonoBehaviour
{
    [Header("스테이지 별 게임 환경을 설정하는 스크립트 스테이지를 불러올때 가장 우선적으로 작동하는 스크립트이다. (적 유닛 종류 등)")]
    [Header("Stage Manager Reference")]
    [SerializeField]
    private StageManager stageManager;
    private UIManager uiManager;

    [Header("Stage Clear Condition")]
    public StageClearState stageClearCondition;
    [Tooltip("KillTargetEnemy 조건일 경우 특정 적의 ID")]
    public int targetEnemyId;
    [Tooltip("SurviveTurn 조건일 경우 생존해야 할 턴 수")]
    public int surviveTurnCount;

    [Space(20)]
    [Header("Player Unit Settings")]
    [Tooltip("플레이어가 사용 가능한 유닛 슬롯의 카운트")]
    [SerializeField]
    private int playerUseUnitSlotCount;
    [Tooltip("플레이어가 사용 가능한 유닛 슬롯의 범위")]
    [SerializeField]
    private int playerUseUnitSlotRange;
    [Tooltip("해당 스테이지에서 플레이어가 사용 가능하도록 배치되어 있는 유닛 리스트")]
    [SerializeField]
    private List<UnitStatus> playerUnitList;

    [Space(20)]
    [Header("Enemy Unit Settings")]
    [Tooltip("해당 스테이지에서 적으로 등장하는 유닛 리스트")]
    [SerializeField]
    private List<UnitStatus> enemyUnitList;

    [Space(20)]
    [Header("UI Elements")]
    [Tooltip("게임 플레이 이전 유닛을 선택하는 UI")]
    [SerializeField]
    public GameObject unitSet_UI;
    [Tooltip("남아있는 선택 가능한 유닛 슬롯 갯수를 보여주는 텍스트")]
    [SerializeField]
    public TMP_Text remainingSetUnitSlotText;

    [Space(5)]
    [Tooltip("게임을 주로 플레이하는 UI")]
    [SerializeField]
    public GameObject play_UI;
    [Tooltip("턴을 지닌 플레이어의 유닛 카드 UI")]
    [SerializeField]
    public UnitCard turnUnitCardUI;
    [Tooltip("현재 타겟이 된 유닛 카드 UI")]
    [SerializeField]
    public UnitCard targetUnitCardUI;
    [Tooltip("타겟 유닛카드를 가리키는 화살표")]
    [SerializeField]
    public GameObject unitCardTargetArrow;
    [Tooltip("스킬 명중률 텍스트")]
    [SerializeField]
    public TMP_Text skillAccuracyText;
    [Tooltip("턴을 지닌 유닛을 표기하는 마커")]
    [SerializeField]
    public GameObject turnUnitMarker;
    [Tooltip("타겟 유닛을 표기하는 마커")]
    [SerializeField]
    public GameObject targetUnitMarker;
    [Tooltip("스테이지 우측 하단의 메뉴 UI")]
    [SerializeField]
    public StageWindowController stageMenuController;
    
    [Space(20)]
    [Header("Skill Settings")]
    [Tooltip("스킬 데이터를 플레이어에게 보여주는 UI 슬롯 리스트")]
    [SerializeField]
    private List<SkillSlotUIController> skillSlotList;

    [Space(20)]
    [Header("Unit Slot Settings")]
    [Tooltip("인 게임에서 유닛이 존재하는 슬롯의 그룹")]
    [SerializeField]
    private UnitSlotGroupController unitSlotGroupController;
    [Tooltip("플레이어가 배치 가능한 유닛이 존재하는 슬롯의 그룹")]
    [SerializeField]
    private UnitSelectSlotGroupController unitSelectSlotsGroupController;


    private void Start()
    {
        stageManager = StageManager.Instance;
        uiManager = UIManager.Instance;
        InitUIObject();

        unitSelectSlotsGroupController.InitUnitSelectSlot();

        PlaceUnitSlot();

        stageManager.skillSlotList = skillSlotList;
        Debug.Log($"skillSlotList.Count {skillSlotList.Count}\n skillSlotList[0] : {skillSlotList[0]}");
        Debug.Log($"gameManager.skillSlotList.Count {stageManager.skillSlotList.Count}\n gameManager.skillSlotList[0] : {stageManager.skillSlotList[0]}");

         
        stageManager.InitGame();
    }

    private void InitUIObject()
    {
        uiManager.turnUnitCardUI = turnUnitCardUI;
        uiManager.targetUnitCardUI = targetUnitCardUI;
        uiManager.skillAccuracyText = skillAccuracyText;
        uiManager.play_UI = play_UI;
        uiManager.unitSet_UI = unitSet_UI;
        uiManager.remainingSetUnitSlotText = remainingSetUnitSlotText;
        uiManager.stageMenuController = stageMenuController;

        stageManager.unitSlotGroupController = unitSlotGroupController;
        stageManager.stageClearCondition = stageClearCondition;
        stageManager.targetEnemyId = targetEnemyId;
        stageManager.surviveTurnCount = surviveTurnCount;

        stageManager.playerUseUnitSlotCount = playerUseUnitSlotCount;
        stageManager.playerUseUnitSlotRange = playerUseUnitSlotRange;
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
}
