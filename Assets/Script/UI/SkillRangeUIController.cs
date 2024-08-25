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
    private GameManager inGameManager;


    private void Start()
    {
        inGameManager = GameManager.Instance;
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
            groundSprite = inGameManager.unitSlots[i].slotGround;

            groundSprite.SetSlotGroundState(SlotGroundState.Normal);
        }

        for (int i = 0; i < skillRange.Length; i++)
        {

            if (inGameManager.currentTurnSlotNumber + skillRange[i] < inGameManager.unitSlots.Count)
            {
                groundSprite = inGameManager.unitSlots[inGameManager.currentTurnSlotNumber + skillRange[i]].slotGround;

                groundSprite.SetSlotGroundState(SlotGroundState.Target);
            }

            if (inGameManager.currentTurnSlotNumber - skillRange[i] >= 0)
            {
                groundSprite = inGameManager.unitSlots[inGameManager.currentTurnSlotNumber - skillRange[i]].slotGround;

                groundSprite.SetSlotGroundState(SlotGroundState.Target);
            }
        }

        groundSprite = inGameManager.unitSlots[inGameManager.currentTurnSlotNumber].slotGround;
        groundSprite.SetSlotGroundState(SlotGroundState.Select);
    }
}
