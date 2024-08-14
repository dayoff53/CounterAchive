using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UnitManager : Singleton<UnitManager>
{
    //���� ������ ���� �������� ������ �ʴ� ������ ���� ������ �����Ͽ� �����ϱ� ��Ʊ� �����̴�.
    [Tooltip("���� ���� ����Ʈ")]
    public List<UnitSlotController> unitSlots;

    [Tooltip("���� ���� ��� ���� ������ ��ȣ")]
    public int currentTurnSlotNumber;

    [SerializeField]
    [Tooltip("�� ������ ���� ��ġ�� ������ ��ųʸ�")]
    private SerializableDictionary<GameObject, Vector3> originalPositions = new SerializableDictionary<GameObject, Vector3>();

    // ������ �̵� �� ���¸� �Ǵ��ϴ� ��
    private bool isMoving = false;

    [Tooltip("������ �������� ���� ������ �� ���� ������ ������ �ѹ�")]
    public int middleNum = 4;

    private void Start()
    {
        SlotPosInit();
    }

    private void SlotPosInit()
    {
        for (int i = 0; i < unitSlots.Count; i++)
        {
            if (unitSlots[i] != null)
            {
                var unit = unitSlots[i].gameObject;
                if (unit != null)
                {
                    // �ʱ� ���� ��ġ ����
                    originalPositions[unit] = unit.transform.position;
                }
            }
        }
    }

    public void UnitInit()
    {
        for(int i = 0; i < unitSlots.Count; i++)
        {
            unitSlots[i].SetUnitState(UnitState.normal);
        }
    }

    /// <summary>
    /// ������ ������ �ٸ� ������ ��ġ�� �̵���ŵ�ϴ�.
    /// </summary>
    /// <param name="moveUnitNum">�̵��� ������ �ε����Դϴ�.</param>
    /// <param name="targetUnitNum">��ǥ ������ �ε����Դϴ�.</param>
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
            isMoving = false; // �̵� �Ϸ� �� �̵� �� ���� ����
        });
    }

    /// <summary>
    /// ���� ���� ���� ������ ������ �Ǵ� �������� �̵���ŵ�ϴ�.
    /// </summary>
    /// <param name="rightMove">���������� �̵����� �����Դϴ�. false�� �������� �̵��մϴ�.</param>
    public void DirectMoveUnit(bool rightMove)
    {
        int moveTargetUnitNum = currentTurnSlotNumber;

        if (isMoving || unitSlots[currentTurnSlotNumber] == null || unitSlots[moveTargetUnitNum] == null)
        {
            Debug.LogWarning("Movement is currently locked or invalid slot numbers provided.");
            return;
        }

        if (currentTurnSlotNumber >= 0 && currentTurnSlotNumber < 4)
        {
            if (rightMove)
            {
                if (currentTurnSlotNumber < middleNum - 1)
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

    public UnitSlotController getTargetSlot(int userNum, int targetRange)
    {
        if(userNum < middleNum)
        {
            return unitSlots[userNum + targetRange];
        }
        else
        {
            return unitSlots[userNum - targetRange];
        }
    }
}

