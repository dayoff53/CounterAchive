using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public partial class StageManager
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
    /// ActionPoint를 상승시키는 스크립트
    /// </summary>
    private void ActionPointUpper(float time)
    {
        foreach (var unitSlot in unitSlotList)
        {
            if (unitSlot.unit != null && unitSlot.isNull == false)
            {
                UnitController unit = unitSlot.unit;
                if (actionPoints.TryGetValue(unit, out float currentPoints))
                {
                    actionPoints[unit] = currentPoints + unit.speed * time;
                    unit.currentActionPoint = actionPoints[unit];

                    CostIncrease(unit, time);

                    if (unit.currentActionPoint >= unit.maxActionPoint)
                    {
                        ExecuteTurn(unitSlot);
                        break;
                    }
                }
                else
                {
                    Debug.LogWarning($"No key found for {unitSlot.name}. Adding key.");
                    actionPoints.Add(unit, unit.speed * time);  // 키가 없을 경우 추가
                    unit.currentActionPoint = actionPoints[unit];
                }
            }
        }
    }


    /// <summary>
    /// 코스트를 지속적으로 상승시키는 스크립트
    /// </summary>
    /// <param name="unit"></param>
    private void CostIncrease(UnitController unit, float time)
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
                        UnitController unit = unitSlot.unit;
                        if (actionPoints.TryGetValue(unit, out float currentPoints))
                        {
                            actionPoints[unit] = currentPoints + unit.speed * skipTime;
                            unit.currentActionPoint = actionPoints[unit];

                            CostIncrease(unit, skipTime);

                            if (unit.currentActionPoint >= unit.maxActionPoint)
                            {
                                isSkip = false;
                                ExecuteTurn(unitSlot);
                                break;
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"No key found for {unit.name}. Adding key.");
                            actionPoints.Add(unit, unit.speed * Time.deltaTime);  // 키가 없을 경우 추가
                            unit.currentActionPoint = actionPoints[unit];
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 다음 턴을 실행할 유닛을 결정합니다.
    /// </summary>
    private void GetNextTurnSlotNumber()
    {
        for (int i = 0; i < unitSlotList.Count; i++)
        {
            if (actionPoints[unitSlotList[i].unit] >= unitSlotList[i].unit.maxActionPoint)
            {
                currentTurnSlotNumber = i;

                UnitSlotController currentTurnSlot = unitSlotList[currentTurnSlotNumber];
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
        currentTurnSlotNumber = unitSlotList.IndexOf(unit);
        UnitSlotController currentTurnSlot = unitSlotList[currentTurnSlotNumber];
        currentTurnSlotIcon.sprite = currentTurnSlot.unit.unitFaceIcon;
        currentTurnName.text = currentTurnSlot.unit.unitName;
        SkillSlotInit(unitSlotList[currentTurnSlotNumber].unit.skillDataList);

        //현재 턴을 가진 유닛 구분
        SlotGroundSpriteController groundSprite = unitSlotList[currentTurnSlotNumber].slotGround;
        groundSprite.SetSlotGroundState(SlotGroundState.Select); 

        //액션 포인트 초기화
        actionPoints[currentTurnSlot.unit] = 0;
        Debug.Log("턴을 시작합니다. 현재 턴은 " + unitSlotList[currentTurnSlotNumber].name + " (" + unitSlotList[currentTurnSlotNumber].unit.speed + " 속도) 유닛입니다.");
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
        unitSlotList[currentTurnSlotNumber].unit.SetAnim(0);

        //UnitGround의 색상 및 스프라이트 초기화
        foreach (var unit in unitSlotList)
        {
            SlotGroundSpriteController groundSprite = unit.slotGround;
            groundSprite.SetSlotGroundState(SlotGroundState.Default);
        }

        currentPrograssState = ProgressState.Stay;
    }
}
