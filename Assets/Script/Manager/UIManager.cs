    // Assets/Script/Manager/UIManager.cs
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class UIManager : Singleton<UIManager>
    {
        [Header("UI Object")]
        public GameObject play_UI;
        public GameObject unitSet_UI;
        public UnitCard turnUnitCardUI;
        public UnitCard targetUnitCardUI;
        public TMP_Text skillAccuracyText;
        public TMP_Text remainingSetUnitSlotText;
        public GameObject turnUnitMarker;
        public GameObject targetUnitMarker;

        [Header("Color Data")]
        public List<Color> unitStateColors;
        public ColorState unitStateColorsObject;

        [Header("Cost UI")]
        public Image costGauge;
        public TMP_Text costText;
        public Image costBar;

        public void UpdateUnitCardUI(bool isPlayer, UnitBase unit)
        {
            UnitStatus changeUnitStatus = new UnitStatus();
            changeUnitStatus.SetStatus(unit);
            if (isPlayer)
                turnUnitCardUI.unitStatus = changeUnitStatus;
            else
                targetUnitCardUI.unitStatus = changeUnitStatus;
        }

        public void SetMarkerPosition(bool isTurnMarker, Transform targetPosition)
        {
            GameObject marker = isTurnMarker ? turnUnitMarker : targetUnitMarker;
            marker.SetActive(true);
            marker.transform.SetParent(targetPosition);
            marker.transform.position = targetPosition.position;
        }

        public void HideMarkers()
        {
            turnUnitMarker.SetActive(false);
            targetUnitMarker.SetActive(false);
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