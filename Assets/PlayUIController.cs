using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayUIController : MonoBehaviour
{
    [Header("StageMaster")]
    [SerializeField]
    private BattleStageMaster battleStageMaster;
    private DataManager dataManager;


    [Space(10)]
    [Header("UnitCard")]
    /// <summary>
    /// 현재 턴 유닛 카드 UI
    /// </summary>
    public UnitCard turnUnitCardUI;

    /// <summary>
    /// 현재 타겟 유닛 카드 UI
    /// </summary>
    public UnitCard targetUnitCardUI;

    /// <summary>
    /// 스킬 정확도 텍스트
    /// </summary>
    public TMP_Text skillAccuracyText;


        [Space(10)]
        [Header("Cost UI")]
        public Image costGauge;
        public TMP_Text costText;
        public Image costBar;
    

    void Start()
    {
        dataManager = DataManager.Instance;
        battleStageMaster = dataManager.battleStageMaster;
    }

    void Reset()
    {
        battleStageMaster = FindObjectOfType<BattleStageMaster>();
    }

    void Init(BattleStageMaster stageMaster)
    {
        this.battleStageMaster = stageMaster;
    }

    /// <summary>
    /// 각 유닛들의 현 상황을 보여주는 UnitCardUI를 관리하는 스크립트
    /// </summary>
    /// <param name="unitNumber"></param>
    public void SetCurrentUnitCardUI(bool isPlayer, int unitNumber)
    {
        if (isPlayer)
        {
            UnitStatus changeUnitStatus = new UnitStatus();
            Debug.Log($"SetCurrentUnitCardUI {battleStageMaster.unitSlotList[unitNumber].unit.unitNumber}");
            changeUnitStatus.SetStatus(dataManager.unitDataList.Find(u => u.unitNumber == battleStageMaster.unitSlotList[unitNumber].unit.unitNumber));
            turnUnitCardUI.unitStatus = changeUnitStatus;
        }
        else
        {
            UnitStatus changeUnitStatus = new UnitStatus();
            changeUnitStatus.SetStatus(dataManager.unitDataList.Find(u => u.unitNumber == battleStageMaster.unitSlotList[unitNumber].unit.unitNumber));
            targetUnitCardUI.unitStatus = changeUnitStatus;
        }
    }

    public void UpdateUnitCardUI(bool isPlayer, UnitBase unit)
    {
        UnitStatus changeUnitStatus = new UnitStatus();
        changeUnitStatus.SetStatus(unit);
        if (isPlayer)
            turnUnitCardUI.unitStatus = changeUnitStatus;
        else
            targetUnitCardUI.unitStatus = changeUnitStatus;
    }

        public void UpdateSkillAccuracy(float accuracy)
        {
            skillAccuracyText.text = $"{accuracy * 100}%";
        }

        
        public void UpdateCostUI(float cost)
        {
            costBar.fillAmount = cost / 10;
            costGauge.fillAmount = cost - Mathf.Floor(cost);
            if (cost > 10)
            {
                costGauge.fillAmount = 1;
            }
            costText.text = Mathf.FloorToInt(cost).ToString();
        }
}
