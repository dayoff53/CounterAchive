using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillRangeUIController : MonoBehaviour
{
    [SerializeField]
    private List<Image> rangeImages;

    [SerializeField]
    private List<Color> skillRangeColors;

    [SerializeField]
    private InGameManager inGameManager;


    private void Start()
    {
        inGameManager = InGameManager.Instance;
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


    public void SetRangeSprite(int[] skillRange)
    {
        SlotGroundSpriteController groundSprite;

        for (int i = 0; i < rangeImages.Count; i++)
        {
            groundSprite = inGameManager.unitSlots[i].slotGroundSpriteController;

            groundSprite.SetSlotGroundState(SlotGroundState.Normal, skillRangeColors[0]);
        }

        for (int i = 0; i < skillRange.Length; i++)
        {

            if (inGameManager.currentTurnSlotNumber + skillRange[i] < inGameManager.unitSlots.Count)
            {
                groundSprite = inGameManager.unitSlots[inGameManager.currentTurnSlotNumber + skillRange[i]].slotGroundSpriteController;

                groundSprite.SetSlotGroundState(SlotGroundState.Target, skillRangeColors[2]);
            }

            if (inGameManager.currentTurnSlotNumber - skillRange[i] >= 0)
            {
                groundSprite = inGameManager.unitSlots[inGameManager.currentTurnSlotNumber - skillRange[i]].slotGroundSpriteController;

                groundSprite.SetSlotGroundState(SlotGroundState.Target, skillRangeColors[2]);
            }
        }

        groundSprite = inGameManager.unitSlots[inGameManager.currentTurnSlotNumber].slotGroundSpriteController;
        groundSprite.SetSlotGroundState(SlotGroundState.Select, skillRangeColors[0]);
    }
}
