using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class InGameManager : Singleton<InGameManager>
{
    // ���� ���� ������� ������ ��ȣ
    public int currentTurnSlotNumber;

    public float cost;

    [Header("UnitSlot")]
    [Tooltip("���� ���� ����Ʈ")]
    public List<UnitSlotController> unitSlots;
    [Tooltip("�� ���� ����Ʈ")]
    public List<UnitSlotController> turnOrder;
    [Tooltip("�ൿ�� ������ ���� Dictionary")]
    private Dictionary<UnitSlotController, float> actionPoints = new Dictionary<UnitSlotController, float>();


    public List<float> currentTurn;


    [Tooltip("���� ���� ���� ���� �ĺ�ǥ")]
    public GameObject currentTurnSlotIcon;


    [Header("SkillSlot")]
    [Tooltip("��ų ���� ����Ʈ")]
    public List<SkillSlotUIController> skillSlot;


    // �� ������ ���� ��ġ�� ������ ��ųʸ�
    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();

    // ������ �̵� �� ���¸� �Ǵ��ϴ� ��
    private bool isMoving = false;

    void Start()
    {
        Init();
    }

    private void Init()
    {
        SlotPosInit();
        TurnCycle();
    }
    public void SetUnitSlot(bool isRight, List<UnitSlotController> setUnitSlots)
    {
        if (isRight)
        {
            for (int i = 0; setUnitSlots.Count > i; i++)
            {
                setUnitSlots[i] = unitSlots[i + 4];
            }
        }
        else
        {
            for (int i = 0; setUnitSlots.Count > i; i++)
            {
                setUnitSlots[i] = unitSlots[i];
            }
        }
    }

    #region Turn
    // �� ������ ��� �� ���� �ѱ�
    private void TurnCycle()
    {
        turnOrder = unitSlots.OrderBy(u => -u.unitData.speed).ToList();

        currentTurnSlotNumber = unitSlots.IndexOf(turnOrder[0]);
        cost += unitSlots[currentTurnSlotNumber].unitData.speed;
        SkillSlotInit(unitSlots[currentTurnSlotNumber].unitData.skillDatas);

        SetTurnSlot();

        Debug.Log("���� �����մϴ�. ���� ���� " + unitSlots[0].name + " (" + unitSlots[0].unitData.speed + " �ӵ�) �����Դϴ�.");
    }

    // Ư�� ���Կ��� ���� �ѱ�
    public void SetTurnSlot()
    {
        // ���� �� ������ GameObject�� ã���ϴ�.
        UnitSlotController currentTurnSlot = unitSlots[currentTurnSlotNumber];

        currentTurnSlot.currentTargetIcon.SetActive(true);
    }

    public IEnumerator DelayTurnEnd(float delay)
    {
        yield return new WaitForSeconds(delay); // ������ �ð���ŭ ���
        TurnEnd(); // ��� �� ȣ���� �Լ�
    }

    // ���� �����Ŵ
    public void TurnEnd()
    {
        unitSlots[currentTurnSlotNumber].SetAnim(0);
        unitSlots[currentTurnSlotNumber].currentTargetIcon.SetActive(false);

        TurnCycle();
    }
    #endregion

    #region SlotMove

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
                if (currentTurnSlotNumber < 3)
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

    #endregion

    #region Skill

    public void SkillSlotInit(List<SkillData> setSkillDatas)
    {
        for (int i = 0; i < setSkillDatas.Count; i++)
        {
            skillSlot[i].SetSkillData(setSkillDatas[i]);
        }
    }

    public void ExecuteAttack(int attackRange, int damage)
    {
        unitSlots[currentTurnSlotNumber + attackRange].unitData.hp -= damage;
    }

    #endregion
}
