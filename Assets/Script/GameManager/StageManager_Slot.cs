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
                var unit = unitSlotList[i].gameObject;
                if (unit != null)
                {
                    // 초기 유닛 슬롯 위치 설정
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
        if (isMoving || unitSlotList[moveUnitNum] == null || unitSlotList[targetUnitNum] == null)
        {
            Debug.LogError("Movement is currently locked or invalid slot numbers provided.");
            return;
        }

        isMoving = true;

        GameObject moveUnit = unitSlotList[moveUnitNum].gameObject;
        GameObject targetUnit = unitSlotList[targetUnitNum].gameObject;

        moveUnit.transform.DOKill();
        targetUnit.transform.DOKill();

        moveUnit.transform.position = originalPositions[moveUnit];
        targetUnit.transform.position = originalPositions[targetUnit];

        Sequence sequence = DOTween.Sequence();
        sequence.Append(moveUnit.transform.DOMove(originalPositions[targetUnit], 0.25f).SetEase(Ease.InOutQuad));
        sequence.Join(targetUnit.transform.DOMove(originalPositions[moveUnit], 0.25f).SetEase(Ease.InOutQuad));
        sequence.OnComplete(() => {
            originalPositions[moveUnit] = moveUnit.transform.position;
            originalPositions[targetUnit] = targetUnit.transform.position;
            UnitSlotController_Old targetSlot = unitSlotList[targetUnitNum];
            unitSlotList[targetUnitNum] = unitSlotList[moveUnitNum];
            unitSlotList[moveUnitNum] = targetSlot;
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
        if(currentPrograssState == ProgressState.UnitPlay || currentPrograssState == ProgressState.SkillTargetSearch)
        {
            int moveTargetUnitNum = currentTurnSlotNumber;

            if (rightMove)
            {
                if (currentTurnSlotNumber < unitSlotList.Count)
                {
                    moveTargetUnitNum = currentTurnSlotNumber + 1;
                }
            }
            else
            {
                if (currentTurnSlotNumber > 0)
                {
                    moveTargetUnitNum = currentTurnSlotNumber - 1;
                }
            }

            if (isMoving || unitSlotList[currentTurnSlotNumber] == null || unitSlotList[moveTargetUnitNum] == null)
            {
                Debug.LogWarning("Movement is currently locked or invalid slot numbers provided.");
                return;
            }

            MoveUnit(currentTurnSlotNumber, moveTargetUnitNum);
            Debug.Log($"moveTargetUnitNum : {moveTargetUnitNum}");
            for (int i = 0; i < unitSlotList.Count; i++)
            {
                unitSlotList[i].slotGround.SetSlotGroundState(SlotGroundState.Default);
            }
            unitSlotList[currentTurnSlotNumber].slotGround.SetSlotGroundState(SlotGroundState.Select);
            currentTurnSlotNumber = moveTargetUnitNum;
            currentPrograssState = ProgressState.UnitPlay;
        }
    }
}
