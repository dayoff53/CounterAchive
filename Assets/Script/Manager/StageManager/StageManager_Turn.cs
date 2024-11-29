using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public partial class StageManager
{
    /// <summary>
    /// actionPoints�� ���������� �� ������ �ӵ���ŭ ������ŵ�ϴ�.
    /// </summary>
    private IEnumerator ActionPointAccumulation()
    {
        while (true)
        {
            switch (currentPrograssState)
            {
                case ProgressState.Stay:
                    ActionPointUpper(Time.deltaTime);
                    break;

                case ProgressState.SkillTargetSearch:
                    break;

                default:
                    break;
            }
            yield return null;
        }
    }


    /// <summary>
    /// ActionPoint�� ��½�Ű�� ��ũ��Ʈ
    /// </summary>
    private void ActionPointUpper(float time)
    {
        foreach (var unitSlot in unitSlotList)
        {
            if (unitSlot.unit != null && unitSlot.isNull == false)
            {
                UnitBase unit = unitSlot.unit;
                if (actionPoints.TryGetValue(unit, out float currentPoints))
                {
                    actionPoints[unit] = currentPoints + unit.speed * time;
                    unit.currentAP = actionPoints[unit];

                    CostIncrease(unit, time);

                    if (unit.currentAP >= unit.maxAp)
                    {
                        ExecuteTurn(unitSlot);
                        break;
                    }
                }
                else
                {
                    Debug.LogWarning($"No key found for {unitSlot.name}. Adding key.");
                    actionPoints.Add(unit, unit.speed * time);  // Ű�� ���� ��� �߰�
                    unit.currentAP = actionPoints[unit];
                }
            }
        }
    }


    /// <summary>
    /// �ڽ�Ʈ�� ���������� ��½�Ű�� ��ũ��Ʈ
    /// </summary>
    /// <param name="unit"></param>
    private void CostIncrease(UnitBase unit, float time)
    {
            if (unit != null)
            {
                if (unit.unitTeam == 1)
                {
                    float currentSpeed = unit.speed;

                    if (cost < 10)
                    {
                        cost += (currentSpeed / 10) * time;
                    }
                    else if (cost >= 10)
                    {
                        cost = 10;
                    }
                }
            }
    }

    public void TurnSkip()
    {
        if (currentPrograssState == ProgressState.Stay)
        {
            bool isSkip = true;
            float skipTime = 0.01f;

            while (isSkip)
            {
                foreach (var unitSlot in unitSlotList)
                {
                    if (unitSlot != null && unitSlot.isNull == false)
                    {
                        UnitBase unit = unitSlot.unit;
                        if (actionPoints.TryGetValue(unit, out float currentPoints))
                        {
                            actionPoints[unit] = currentPoints + unit.speed * skipTime;
                            unit.currentAP = actionPoints[unit];

                            CostIncrease(unit, skipTime);

                            if (unit.currentAP >= unit.maxAp)
                            {
                                isSkip = false;
                                ExecuteTurn(unitSlot);
                                break;
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"No key found for {unit.name}. Adding key.");
                            actionPoints.Add(unit, unit.speed * Time.deltaTime);  // Ű�� ���� ��� �߰�
                            unit.currentAP = actionPoints[unit];
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// ���� ���� ������ ������ �����մϴ�.
    /// </summary>
    private void GetNextTurnSlotNumber()
    {
        for (int i = 0; i < unitSlotList.Count; i++)
        {
            if (actionPoints[unitSlotList[i].unit] >= unitSlotList[i].unit.maxAp)
            {
                currentTurnSlotNumber = i;

                UnitSlotController currentTurnSlot = unitSlotList[currentTurnSlotNumber];
                ExecuteTurn(currentTurnSlot);
                break;
            }
        }
    }

    /// <summary>
    /// ������ ���� �����մϴ�.
    /// </summary>
    private void ExecuteTurn(UnitSlotController unit)
    {
        currentPrograssState = ProgressState.UnitPlay;

        //�ʱ�ȭ
        currentTurnSlotNumber = unitSlotList.IndexOf(unit);
        UnitBase currentTurnUnit = unit.unit;
        SetCurrentUnitCardUI(true, currentTurnSlotNumber);
        SkillSlotInit(unitSlotList[currentTurnSlotNumber].unit.skillDataList);


        //���� ���� ���� ���� ����
        SlotGroundSpriteController groundSprite = unitSlotList[currentTurnSlotNumber].slotGround;
        groundSprite.SetSlotGroundState(SlotGroundState.Select); 

        //�׼� ����Ʈ �ʱ�ȭ
        actionPoints[currentTurnUnit] = 0;
        Debug.Log("���� �����մϴ�. ���� ���� " + unitSlotList[currentTurnSlotNumber].name + " (" + unitSlotList[currentTurnSlotNumber].unit.speed + " �ӵ�) �����Դϴ�.");
    }

    /// <summary>
    /// �� ������ �� ��Ȳ�� �����ִ� UnitCardUI�� �����ϴ� ��ũ��Ʈ
    /// </summary>
    /// <param name="unitNumber"></param>
    public void SetCurrentUnitCardUI(bool isPlayer, int unitNumber)
    {
        if (isPlayer)
        {
            UnitStatus changeUnitStatus = new UnitStatus();
            changeUnitStatus.SetStatus(dataManager.unitDataList.Find(u => u.unitNumber == unitSlotList[unitNumber].unit.unitNumber));
            currentPlayerUnitCardUI.unitStatus = changeUnitStatus;
        }
        else
        {
            UnitStatus changeUnitStatus = new UnitStatus();
            changeUnitStatus.SetStatus(dataManager.unitDataList.Find(u => u.unitNumber == unitSlotList[unitNumber].unit.unitNumber));
            currentTargetUnitCardUI.unitStatus = changeUnitStatus;
        }
    }

    /// <summary>
    /// ���� �ð� ��� �� ���� �����մϴ�. 
    /// </summary>
    /// <param name="delay">���ð�</param>
    public void DelayTurnEnd(float delay)
    {
        StartCoroutine(_DelayTurnEnd(delay));
    }
    public IEnumerator _DelayTurnEnd(float delay)
    {
        yield return new WaitForSeconds(delay); // ������ �ð���ŭ ���
        TurnEnd(); // ��� �� ȣ���� �Լ�
    }

    /// <summary>
    /// ���� �����մϴ�.
    /// </summary>
    public void TurnEnd()
    {
        unitSlotList[currentTurnSlotNumber].unit.SetAnim(0);

        //UnitGround�� ���� �� ��������Ʈ �ʱ�ȭ
        foreach (var unit in unitSlotList)
        {
            SlotGroundSpriteController groundSprite = unit.slotGround;
            groundSprite.SetSlotGroundState(SlotGroundState.Default);
        }

        currentPrograssState = ProgressState.Stay;
    }
}
