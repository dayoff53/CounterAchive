using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public partial class GameManager
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
                foreach (var unit in unitSlots)
                {
                    if (unit != null && unit.isNull == false)
                    {
                        if (actionPoints.TryGetValue(unit, out float currentPoints))
                        {
                            actionPoints[unit] = currentPoints + unit.speed * Time.deltaTime;
                            unit.currentActionPoint = actionPoints[unit];

                            CostIncrease(unit);
                        }
                        else
                        {
                            Debug.LogWarning($"No key found for {unit.name}. Adding key.");
                            actionPoints.Add(unit, unit.speed * Time.deltaTime);  // Ű�� ���� ��� �߰�
                            unit.currentActionPoint = actionPoints[unit];
                        }
                    }
                }
                GetNextTurnSlotNumber();
                    break;

                case ProgressState.SkillSelect:
                    break;

                default:
                    break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// �ڽ�Ʈ�� ���������� ��½�Ű�� ��ũ��Ʈ
    /// </summary>
    /// <param name="unit"></param>
    private void CostIncrease(UnitSlotController unit)
    {
            if (unit != null)
            {
                if (unit.unitTeam == 1)
                {
                    float currentSpeed = unit.speed;

                    if (cost < 10)
                    {
                        cost += (currentSpeed / 20) * Time.deltaTime;
                    }
                    else if (cost >= 10)
                    {
                        cost = 10;
                    }
                }
            }
        
    }

    /// <summary>
    /// ���� ���� ������ ������ �����մϴ�.
    /// </summary>
    private void GetNextTurnSlotNumber()
    {
        for (int i = 0; i < unitSlots.Count; i++)
        {
            if (actionPoints[unitSlots[i]] >= unitSlots[i].maxActionPoint)
            {
                currentTurnSlotNumber = i;

                UnitSlotController currentTurnSlot = unitSlots[currentTurnSlotNumber];
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
        currentTurnSlotNumber = unitSlots.IndexOf(unit);
        UnitSlotController currentTurnSlot = unitSlots[currentTurnSlotNumber];
        currentTurnSlotIcon.sprite = currentTurnSlot.unitFaceIcon;
        currentTurnName.text = currentTurnSlot.unitName;
        SkillSlotInit(unitSlots[currentTurnSlotNumber].skillDatas);

        //���� ���� ���� ���� ����
        SlotGroundSpriteController groundSprite = unitSlots[currentTurnSlotNumber].slotGround;
        groundSprite.SetSlotGroundState(SlotGroundState.Select); 

        //�׼� ����Ʈ �ʱ�ȭ
        actionPoints[currentTurnSlot] = 0;
        Debug.Log("���� �����մϴ�. ���� ���� " + unitSlots[0].name + " (" + unitSlots[0].speed + " �ӵ�) �����Դϴ�.");
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
        unitSlots[currentTurnSlotNumber].SetAnim(0);

        //UnitGround�� ���� �� ��������Ʈ �ʱ�ȭ
        foreach (var unit in unitSlots)
        {
            SlotGroundSpriteController groundSprite = unit.slotGround;
            groundSprite.SetSlotGroundState(SlotGroundState.Default);
        }

        currentPrograssState = ProgressState.Stay;
    }
}
