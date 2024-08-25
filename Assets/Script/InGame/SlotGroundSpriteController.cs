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
    private SlotGroundState slotGroundState
    {
        get { return _slotGroundState; }
        set
        {
            _slotGroundState = value;

            switch (value)
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

    [SerializeField]
    private SpriteRenderer groundSpriteRenderer;

    [SerializeField]
    private List<Sprite> groundSprite;

    private GameManager gameManager;


    private void Start()
    {
        gameManager = GameManager.Instance;
    }

    public void SetSlotGroundState(SlotGroundState setSlotGroundState)
    {
        slotGroundState = setSlotGroundState;

        groundSpriteRenderer.color = gameManager.unitStateColors[unitSlot.unitTeam];
    }

    public void OnMouseDown()
    {
        switch(gameManager.currentPrograssState)
        {
            case ProgressState.SkillSelect:
                if (gameManager.cost >= gameManager.currentSkillSlot.skillData.skillCost)
                {
                    gameManager.skillTargetNum = unitSlot.slotNum;
                    gameManager.SkillStart();
                }
                else
                {
                    Debug.Log("Not enough costs");
                }
                break;

            case ProgressState.UnitSelect:
                if(unitSlot.isNull == true && gameManager.currentSelectUnitData.unitName != "Null")
                {
                    unitSlot.ChangeUnit(gameManager.currentSelectUnitData, 1);
                    gameManager.playerUseUnitSlotCount--;
                    gameManager.UnitSetGame();
                }
                break;

            default:
                break;
        }
    }
}
