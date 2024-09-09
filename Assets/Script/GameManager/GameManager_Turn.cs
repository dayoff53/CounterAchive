using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public partial class GameManager
{
    /// <summary>
    /// actionPoints를 지속적으로 각 유닛의 속도만큼 증가시킵니다.
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
                            actionPoints.Add(unit, unit.speed * Time.deltaTime);  // 키가 없을 경우 추가
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
    /// 코스트를 지속적으로 상승시키는 스크립트
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
    /// 다음 턴을 실행할 유닛을 결정합니다.
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
    /// 유닛의 턴을 실행합니다.
    /// </summary>
    private void ExecuteTurn(UnitSlotController unit)
    {
        currentPrograssState = ProgressState.UnitPlay;

        //초기화
        currentTurnSlotNumber = unitSlots.IndexOf(unit);
        UnitSlotController currentTurnSlot = unitSlots[currentTurnSlotNumber];
        currentTurnSlotIcon.sprite = currentTurnSlot.unitFaceIcon;
        currentTurnName.text = currentTurnSlot.unitName;
        SkillSlotInit(unitSlots[currentTurnSlotNumber].skillDatas);

        //현재 턴을 가진 유닛 구분
        SlotGroundSpriteController groundSprite = unitSlots[currentTurnSlotNumber].slotGround;
        groundSprite.SetSlotGroundState(SlotGroundState.Select); 

        //액션 포인트 초기화
        actionPoints[currentTurnSlot] = 0;
        Debug.Log("턴을 시작합니다. 현재 턴은 " + unitSlots[0].name + " (" + unitSlots[0].speed + " 속도) 유닛입니다.");
    }

    /// <summary>
    /// 일정 시간 대기 후 턴을 종료합니다. 
    /// </summary>
    /// <param name="delay">대기시간</param>
    public void DelayTurnEnd(float delay)
    {
        StartCoroutine(_DelayTurnEnd(delay));
    }
    public IEnumerator _DelayTurnEnd(float delay)
    {
        yield return new WaitForSeconds(delay); // 지정된 시간만큼 대기
        TurnEnd(); // 대기 후 호출할 함수
    }

    /// <summary>
    /// 턴을 종료합니다.
    /// </summary>
    public void TurnEnd()
    {
        unitSlots[currentTurnSlotNumber].SetAnim(0);

        //UnitGround의 색상 및 스프라이트 초기화
        foreach (var unit in unitSlots)
        {
            SlotGroundSpriteController groundSprite = unit.slotGround;
            groundSprite.SetSlotGroundState(SlotGroundState.Default);
        }

        currentPrograssState = ProgressState.Stay;
    }
}
