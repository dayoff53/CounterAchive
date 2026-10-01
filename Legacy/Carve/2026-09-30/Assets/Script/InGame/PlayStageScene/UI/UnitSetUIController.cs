using UnityEngine;
using TMPro;

public class UnitSetUIController : MonoBehaviour
{
        /// <summary>
        /// 남은 유닛 슬롯 텍스트 (기능이 메우 적음으로 추후 기능 합병할 것)
        /// </summary>
        public TMP_Text remainingSetUnitSlotText;

        public void UpdateRemainingSlots(int count)
        {
            remainingSetUnitSlotText.text = $"RemainingUnitSlot : {count}";
        }
}
