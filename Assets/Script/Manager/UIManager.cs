    using System.Collections;   
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    
    public class UIManager : Singleton<UIManager>
    {
        /// <summary>
        /// StageUI 우측 하단에 위치하는 각종 데이터들을 표기하는 공간의 컴포넌트
        /// </summary>
        public StageWindowController stageMenuController;

        [Space(10)]
        [Header("UI Object")]
        public GameObject play_UI;
        public GameObject unitSet_UI;
        public GameObject win_UI;
        public WinUIController winUIController;
        public UnitCard turnUnitCardUI;
        public UnitCard targetUnitCardUI;
        public TMP_Text skillAccuracyText;
        public TMP_Text remainingSetUnitSlotText;
        public Image fadeProdutionPanel;

        [Space(10)]
        [Header("Cost UI")]
        public Image costGauge;
        public TMP_Text costText;
        public Image costBar;
    
        [Space(10)]
        [Header("Color Data")]
        public List<Color> unitStateColors;
        public ProdutionState unitStateColorsObject;

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

        public void UpdateRemainingSlots(int count)
        {
            remainingSetUnitSlotText.text = $"RemainingUnitSlot : {count}";
        }

        public void SwitchUIMode(bool isPlayMode)
        {
            play_UI.SetActive(isPlayMode);
            unitSet_UI.SetActive(!isPlayMode);
            
            if(!isPlayMode)
            {
                win_UI.SetActive(false);
            }
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