using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UnitSlot ¹Ù´Ú¿¡ ±ò¸®´Â SlotGround (1.defalut, 2.select, 3.target)
/// </summary>
public enum SlotGroundState
{
    Default,
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
                case SlotGroundState.Default:
                    groundSpriteRenderer.sprite = groundSpriteList[0];
                    break;
                case SlotGroundState.Select:
                    groundSpriteRenderer.sprite = groundSpriteList[1];
                    break;
                case SlotGroundState.Target:
                    groundSpriteRenderer.sprite = groundSpriteList[2];
                    break;
            }
        }
    }

    [SerializeField]
    private SpriteRenderer groundSpriteRenderer;

    [SerializeField]
    private List<Sprite> groundSpriteList;

    private StageManager gameManager;


    private void Start()
    {
        gameManager = StageManager.Instance;
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
            case ProgressState.SkillTargetSearch:
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
                if(unitSlot.isNull == true && gameManager.currentSelectUnitState.name != "Null" && unitSlot.unitTeam == 1)
                {
                    unitSlot.ChangeUnit(gameManager.currentSelectUnitState, 1);
                    gameManager.playerUseUnitSlotCount--;
                    gameManager.UnitSetGame();
                }
                break;

            default:
                break;
        }
    }
}
