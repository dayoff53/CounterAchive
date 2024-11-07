using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SkillSlot의 UI
/// </summary>
public class SkillRangeUIController : MonoBehaviour
{
    [SerializeField]
    private List<Image> rangeImages;

    [SerializeField]
    private List<Color> skillRangeColors;

    [SerializeField]
    private StageManager stageManager;


    private void Start()
    {
        stageManager = StageManager.Instance;
    }

    public void SkillRangeInit(int[] skillRange)
    {
        for (int i = 0; i < rangeImages.Count; i++)
        {
            rangeImages[i].color = skillRangeColors[0];
        }

        rangeImages[0].color = skillRangeColors[1];

        for (int i = 0; i < skillRange.Length; i++)
        {
            rangeImages[skillRange[i]].color = skillRangeColors[2];
        }
    }

    /// <summary>
    /// 스킬 사거리만큼 groundSprite를 변경
    /// </summary>
    /// <param name="skillRange"></param>
    public void SetRangeGround(int[] skillRange)
    {
        SlotGroundSpriteController groundSprite;

        for (int i = 0; i < rangeImages.Count; i++)
        {
            groundSprite = stageManager.unitSlotList[i].slotGround;

            groundSprite.SetSlotGroundState(SlotGroundState.Default);
        }

        for (int i = 0; i < skillRange.Length; i++)
        {

            if (stageManager.currentTurnSlotNumber + skillRange[i] < stageManager.unitSlotList.Count)
            {
                groundSprite = stageManager.unitSlotList[stageManager.currentTurnSlotNumber + skillRange[i]].slotGround;

                groundSprite.SetSlotGroundState(SlotGroundState.Target);
            }

            if (stageManager.currentTurnSlotNumber - skillRange[i] >= 0)
            {
                groundSprite = stageManager.unitSlotList[stageManager.currentTurnSlotNumber - skillRange[i]].slotGround;

                groundSprite.SetSlotGroundState(SlotGroundState.Target);
            }
        }

        groundSprite = stageManager.unitSlotList[stageManager.currentTurnSlotNumber].slotGround;
        groundSprite.SetSlotGroundState(SlotGroundState.Select);
    }
}
