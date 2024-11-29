using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 스테이지 별 게임 환경을 설정하는 스크립트. 적과 아군의 배치, 스테이지의 패베 및 승리 조건, gameManager에 스테이지 관련 데이터 보내기 등, 스테이지를 불러올때 가장 우선적으로 작동하여 스테이지 환경을 구성하는 스크립트이다.
/// </summary>
public class StageMaster : MonoBehaviour
{
    [Header("스테이지 별 게임 환경을 설정하는 스크립트 스테이지를 불러올때 가장 우선적으로 작동하는 스크립트이다. (적 유닛 종류 등)")]

    [SerializeField]
    private StageManager stageManager;

    /// <summary>
    /// 플레이어가 사용 가능한 유닛 슬롯의 카운트
    /// </summary>
    [SerializeField]
    private int playerUseUnitSlotCount;

    /// <summary>
    /// 플레이어가 사용 가능한 유닛 슬롯의 범위
    /// </summary>
    [SerializeField]
    private int playerUseUnitSlotRange;

    /// <summary>
    /// 해당 스테이지에서 플레이어가 사용 가능하도록 배치되어 있는 유닛 리스트
    /// </summary>
    [SerializeField]
    private List<UnitStatus> playerUnitList;

    /// <summary>
    /// 해당 스테이지에서 적으로 등장하는 유닛 리스트
    /// </summary>
    [SerializeField]
    private List<UnitStatus> enemyUnitList;

    /// <summary>
    /// 스킬 데이터를 플레이어에게 보여주는 UI 슬롯 리스트
    /// </summary>
    [SerializeField]
    private List<SkillSlotUIController> skillSlotList;


    /// <summary>
    /// 인 게임에서 유닛이 존재하는 슬롯의 그룹
    /// </summary>
    [SerializeField]
    private UnitSlotGroupController unitSlotGroupController;

    /// <summary>
    /// 플레이어가 배치 가능한 유닛이 존재하는 슬롯의 그룹
    /// </summary>
    [SerializeField]
    private UnitSelectSlotGroupController unitSelectSlotsGroupController;



    [Tooltip("현재 턴을 지닌 플레이어의 유닛 카드 UI")]
    [SerializeField]
    public UnitCardController currentPlayerUnitCardUI;

    [Tooltip("현재 타겟이 된 유닛 카드 UI")]
    [SerializeField]
    public UnitCardController currentTargetUnitCardUI;

    [Tooltip("타겟 유닛카드를 가리키는 화살표")]
    [SerializeField]
    public Image unitCardTargetArrow;

    [Tooltip("게임을 주로 플래이하는 UI")]
    [SerializeField]
    public GameObject play_UI;

    [Tooltip("게임 플레이 이전 유닛을 선택하는 UI")]
    [SerializeField]
    public GameObject unitSet_UI;

    [Tooltip("남아있는 선택 가능한 유닛 슬롯 갯수를 보여주는 텍스트")]
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
            unitSlotGroupController.unitSlots[endNum - i].SetUnit(enemyUnitList[i], 2);
        }
    }
}
