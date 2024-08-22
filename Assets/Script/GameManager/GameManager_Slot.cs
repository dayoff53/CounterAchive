using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;
using UnityEngine;

public partial class GameManager
{
    private void SlotPosInit()
    {
        for (int i = 0; i < unitSlots.Count; i++)
        {
            if (unitSlots[i] != null)
            {
                var unit = unitSlots[i].gameObject;
                if (unit != null)
                {
                    // 초기 유닛 위치 설정
                    originalPositions[unit] = unit.transform.position;
                }
            }
        }
    }

    /// <summary>
    /// 지정된 유닛을 다른 유닛의 위치로 이동시킵니다.
    /// </summary>
    /// <param name="moveUnitNum">이동할 유닛의 인덱스입니다.</param>
    /// <param name="targetUnitNum">목표 유닛의 인덱스입니다.</param>
    public void MoveUnit(int moveUnitNum, int targetUnitNum)
    {
        if (isMoving || unitSlots[moveUnitNum] == null || unitSlots[targetUnitNum] == null)
        {
            Debug.LogError("Movement is currently locked or invalid slot numbers provided.");
            return;
        }

        isMoving = true;

        GameObject moveUnit = unitSlots[moveUnitNum].gameObject;
        GameObject targetUnit = unitSlots[targetUnitNum].gameObject;

        moveUnit.transform.DOKill();
        targetUnit.transform.DOKill();

        moveUnit.transform.position = originalPositions[moveUnit];
        targetUnit.transform.position = originalPositions[targetUnit];

        Sequence sequence = DOTween.Sequence();
        sequence.Append(moveUnit.transform.DOMove(originalPositions[targetUnit], 1f).SetEase(Ease.InOutQuad));
        sequence.Join(targetUnit.transform.DOMove(originalPositions[moveUnit], 1f).SetEase(Ease.InOutQuad));
        sequence.OnComplete(() => {
            originalPositions[moveUnit] = moveUnit.transform.position;
            originalPositions[targetUnit] = targetUnit.transform.position;
            UnitSlotController targetSlot = unitSlots[targetUnitNum];
            unitSlots[targetUnitNum] = unitSlots[moveUnitNum];
            unitSlots[moveUnitNum] = targetSlot;
            Debug.Log("Units have been swapped successfully.");
            isMoving = false; // 이동 완료 후 이동 중 상태 해제
        });
    }

    /// <summary>
    /// 현재 턴을 가진 유닛을 오른쪽 또는 왼쪽으로 이동시킵니다.
    /// </summary>
    /// <param name="rightMove">오른쪽으로 이동할지 여부입니다. false면 왼쪽으로 이동합니다.</param>
    public void DirectMoveUnit(bool rightMove)
    {
        int moveTargetUnitNum = currentTurnSlotNumber;

        if (isMoving || unitSlots[currentTurnSlotNumber] == null || unitSlots[moveTargetUnitNum] == null)
        {
            Debug.LogWarning("Movement is currently locked or invalid slot numbers provided.");
            return;
        }

        if (rightMove)
        {
            if (currentTurnSlotNumber < unitSlots.Count)
            {
                MoveUnit(currentTurnSlotNumber, currentTurnSlotNumber + 1);

                currentTurnSlotNumber = currentTurnSlotNumber + 1;
            }
        }
        else
        {
            if (currentTurnSlotNumber > 0)
            {
                MoveUnit(currentTurnSlotNumber, currentTurnSlotNumber - 1);

                currentTurnSlotNumber = currentTurnSlotNumber - 1;
            }
        }
    }

}
