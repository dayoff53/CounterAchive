using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UnitSlot ¹Ù´Ú¿¡ ±ò¸®´Â SlotGround (1.defalut, 2.select, 3.target)
/// </summary>
public enum SlotGroundState
{
    Normal,
    Select,
    Target
}

public class SlotGroundSpriteController : MonoBehaviour
{
    [SerializeField]
    private UnitSlotController unitSlot;

    [SerializeField]
    [Tooltip("1.defalut, 2.select, 3.target ")]
    private SlotGroundState _slotGroundState;

    [SerializeField]
    private SpriteRenderer groundSpriteRenderer;

    [SerializeField]
    private List<Sprite> groundSprite;

    private InGameManager inGameManager;


    private void Start()
    {
        inGameManager = InGameManager.Instance;
    }

    private SlotGroundState slotGroundState
    {
        get { return _slotGroundState; }
        set
        {
            _slotGroundState = value;

            switch (slotGroundState)
            {
                case SlotGroundState.Normal:
                    groundSpriteRenderer.sprite = groundSprite[0];
                    break;
                case SlotGroundState.Select:
                    groundSpriteRenderer.sprite = groundSprite[1];
                    break;
                case SlotGroundState.Target:
                    groundSpriteRenderer.sprite = groundSprite[2];
                    break;
            }
        }
    }


    public void SetSlotGroundState(SlotGroundState setSlotGroundState, Color groundColor)
    {
        slotGroundState = setSlotGroundState;
        groundSpriteRenderer.color = groundColor;
    }

    public void OnMouseDown()
    {
        if (inGameManager.currentTurn == turnState.SkillSelect)
        {
            if (inGameManager.cost >= inGameManager.currentSkillSlot.skillData.skillCost)
            {
                inGameManager.currentTurn = turnState.SkillPlay;


                inGameManager.unitSlots[inGameManager.currentTurnSlotNumber].SetAnim(1);
                StartCoroutine(inGameManager.DelayTurnEnd(inGameManager.currentSkillSlot.skillData.skillEndTime));
                inGameManager.currentSkillSlot.OnSkillPlay(unitSlot.slotNum);
            }
            else
            {
                Debug.Log("Not enough costs");
            }
        }
    }
}
