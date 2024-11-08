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
                    // �ʱ� ���� ���� ��ġ ����
                    originalPositions[unit] = unit.transform.position;
                }
            }
        }
    }

    /// <summary>
    /// ������ ������ �ٸ� ������ ��ġ�� �̵���ŵ�ϴ�.
    /// </summary>
    /// <param name="moveUnitNum">�̵��� ������ �ε����Դϴ�.</param>
    /// <param name="targetUnitNum">��ǥ ������ �ε����Դϴ�.</param>
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

            isMoving = false; // �̵� �Ϸ� �� �̵� �� ���� ����
        });
    }

    /// <summary>
    /// ���� ���� ���� ������ ������ �Ǵ� �������� �̵���ŵ�ϴ�.
    /// </summary>
    /// <param name="rightMove">���������� �̵����� �����Դϴ�. false�� �������� �̵��մϴ�.</param>
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
