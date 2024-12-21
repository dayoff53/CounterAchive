using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// UnitSlot 바닥에 깔리는 SlotGround (1.defalut, 2.select, 3.target)
/// </summary>
public enum SlotGroundState
{
    Default,
    Select,
    Target
}

/// <summary>
/// 각 유닛의 클릭 입력값을 담당하는 Ground 기능을 담당하는 클래스
/// </summary>
public class SlotGroundSpriteController : MonoBehaviour
{
    [SerializeField]
    private UnitSlotController unitSlot;

    private SlotGroundState _slotGroundState;
    [SerializeField]
    [Tooltip("1.defalut, 2.select, 3.target ")]
    private SlotGroundState slotGroundState
    {
        get
        {
            return _slotGroundState;
        }

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
        Debug.Log($"{gameObject.name}.setSlotGroundState = { slotGroundState }");

        groundSpriteRenderer.color = stageManager.unitStateColors[unitSlot.unitTeam];
    }

    public void OnMouseDown()
    {
        switch(stageManager.currentPrograssState)
        {
            case ProgressState.SkillTargetSearch:

                switch(slotGroundState)
                {
                    case SlotGroundState.Target:
                        stageManager.SkillTargetSelect(unitSlot);
                        break;
                }
                break;

            case ProgressState.UnitSelect:

                if(unitSlot.isNull == true && stageManager.currentSelectUnitState.unitName != "Null" && unitSlot.unitTeam == 1)
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
