using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;
using UnityEngine;

public partial class StageManager
{
    private void SlotPosInit()
    {
        for (int i = 0; i < unitSlotList.Count; i++)
        {
            if (unitSlotList[i] != null)
            {
                GameObject unit = unitSlotList[i].gameObject;
                if (unit != null)
                {
                    // 초기 유닛 슬롯 위치 설정
                    originalPositions[i] = unit.transform.position;
                }
            }
        }
    }

    /// <summary>
    /// 지정된 유닛을 다른 유닛의 위치로 이동시킵니다.
    /// </summary>
    public void MoveUnits()
    {
        for (int i = 0; i < unitSlotList.Count; i++)
        {
            GameObject moveSlot = unitSlotList[i].gameObject;


            Tween tween = DOTween.Sequence();
            tween = (moveSlot.transform.DOMove(originalPositions[i], 0.25f).SetEase(Ease.InOutQuad));
            tween.OnComplete(() =>
            {
                moveSlot.transform.position = originalPositions[i];
            });
        }
    }

    /// <summary>
    /// 현재 턴을 가진 유닛을 오른쪽 또는 왼쪽으로 이동시킵니다.
    /// </summary>
    /// <param name="rightMove">오른쪽으로 이동할지 여부입니다. false면 왼쪽으로 이동합니다.</param>
    public void DirectMoveUnit(bool rightMove)
    {
        if(currentPrograssState == ProgressState.UnitPlay || currentPrograssState == ProgressState.SkillTargetSearch)
        {
            UnitSlotController changeSlot = null;

            if (rightMove)
            {
                if (currentTurnSlotNumber < unitSlotList.Count - 1)
                {
                    changeSlot = unitSlotList[currentTurnSlotNumber];
                    unitSlotList[currentTurnSlotNumber] = unitSlotList[currentTurnSlotNumber + 1];
                    unitSlotList[currentTurnSlotNumber + 1] = changeSlot;
                    currentTurnSlotNumber = currentTurnSlotNumber + 1;
                }
            }
            else
            {
                if (currentTurnSlotNumber > 0)
                {
                    changeSlot = unitSlotList[currentTurnSlotNumber];
                    unitSlotList[currentTurnSlotNumber] = unitSlotList[currentTurnSlotNumber - 1];
                    unitSlotList[currentTurnSlotNumber - 1] = changeSlot;
                    currentTurnSlotNumber = currentTurnSlotNumber - 1;
                }
            }

            /*
            if (isUnitMoving || unitSlotList[currentTurnSlotNumber] == null || unitSlotList[moveTargetUnitNum] == null)
            {
                Debug.LogWarning("Movement is currently locked or invalid slot numbers provided.");
                return;
            }
            */

            MoveUnits();

            for (int i = 0; i < unitSlotList.Count; i++)
            {
                unitSlotList[i].slotGround.SetSlotGroundState(SlotGroundState.Default);
            }
            unitSlotList[currentTurnSlotNumber].slotGround.SetSlotGroundState(SlotGroundState.Select);
            currentPrograssState = ProgressState.UnitPlay;
        }
    }
}
