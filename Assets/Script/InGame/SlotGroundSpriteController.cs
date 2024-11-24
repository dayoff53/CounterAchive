using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

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

    private StageManager stageManager;


    private void Start()
    {
        stageManager = StageManager.Instance;
    }

    public void SetSlotGroundState(SlotGroundState setSlotGroundState)
    {
        slotGroundState = setSlotGroundState;

        groundSpriteRenderer.color = stageManager.unitStateColors[unitSlot.unit.unitTeam];
    }

    public void OnMouseDown()
    {
        switch(stageManager.currentPrograssState)
        {
            case ProgressState.SkillTargetSearch:
                if (stageManager.cost >= stageManager.currentSkillSlot.skillData.skillCost)
                {
                    stageManager.skillTargetNum = unitSlot.slotNumber;
                    stageManager.SkillStart();
                }
                else
                {
                    Debug.Log("Not enough costs");
                }
                break;

            case ProgressState.UnitSelect:
                if(unitSlot.isNull == true && stageManager.currentSelectUnitState.unitName != "Null" && unitSlot.unit.unitTeam == 1)
                {
                    unitSlot.SetUnit(stageManager.currentSelectUnitState, 1);
                    stageManager.playerUseUnitSlotCount--;
                    stageManager.UnitSetGame();
                }
                break;

            default:
                break;
        }
    }
}
