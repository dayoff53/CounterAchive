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

    [SerializeField]
    [Tooltip("1.defalut, 2.select, 3.target ")]
    private SlotGroundState slotGroundState
    {
        set
        {
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
                if (stageManager.cost >= stageManager.currentSkillData.skillCost)
                {
                    if (stageManager.skillTargetNum == stageManager.unitSlotList.IndexOf(unitSlot))
                    {
                        stageManager.SkillStart();
                    }

                    int targetNum = stageManager.unitSlotList.IndexOf(unitSlot);
                    stageManager.skillTargetNum = targetNum;
                    stageManager.SetCurrentUnitCardUI(false, targetNum);
                    stageManager.targetUnitMarker.SetActive(true);
                    stageManager.targetUnitMarker.transform.parent = unitSlot.unit.hitPosition.transform; 
                    stageManager.targetUnitMarker.transform.position = unitSlot.unit.hitPosition.transform.position;
                    Debug.Log($"stageManager.skillTargetNum = {stageManager.skillTargetNum}");
                }
                else
                {
                    stageManager.SetCurrentUnitCardUI(false, 0);
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
