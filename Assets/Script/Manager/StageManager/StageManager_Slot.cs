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
                    // �ʱ� ���� ���� ��ġ ����
                    slotOriginalPositions[i] = unit.transform.position;
                }
            }
        }
    }

    /// <summary>
    /// ������ ������ �ٸ� ������ ��ġ�� �̵���ŵ�ϴ�.
    /// </summary>
    public void MoveUnits()
    {
        for (int i = 0; i < unitSlotList.Count; i++)
        {
            GameObject moveSlot = unitSlotList[i].gameObject;


            Tween tween = DOTween.Sequence();
            tween = (moveSlot.transform.DOMove(slotOriginalPositions[i], 0.25f).SetEase(Ease.InOutQuad));
            tween.OnComplete(() =>
            {
                moveSlot.transform.position = slotOriginalPositions[i];
            });
        }
    }

    /// <summary>
    /// ���� ���� ���� ������ ������ �Ǵ� �������� �̵���ŵ�ϴ�.
    /// </summary>
    /// <param name="rightMove">���������� �̵����� �����Դϴ�. false�� �������� �̵��մϴ�.</param>
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
