using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public partial class StageManager
{
    #region TurnVariable
    [Space(10)]
    [Header("Turn Data")]
    [Tooltip("행동력 누적을 위한 Dictionary")]
    private SerializableDictionary<UnitBase, float> actionPoints = new SerializableDictionary<UnitBase, float>();


    /// <summary>
    /// 현재 턴을 행사 중인 슬롯의 번호
    /// </summary>
    public int currentTurnSlotNumber;
    #endregion


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
                UnitBase unit = unitSlot.unit;
                if (actionPoints.TryGetValue(unit, out float currentPoints))
                {
                    actionPoints[unit] = currentPoints + unit.speed * time;
                    unit.currentAp = actionPoints[unit];

                    CostIncrease(unit, time);

                    if (unit.currentAp >= unit.maxAp)
                    {
                        ExecuteTurn(unitSlot);
                        break;
                    }
                }
                else
                {
                    Debug.LogWarning($"No key found for {unitSlot.name}. Adding key.");
                    actionPoints.Add(unit, unit.speed * time);  // 키가 없을 경우 추가
                    unit.currentAp = actionPoints[unit];
                }
            }
        }
    }


    /// <summary>
    /// 코스트를 지속적으로 상승시키는 스크립트
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
                            unit.currentAp = actionPoints[unit];

                            CostIncrease(unit, skipTime);

                            if (unit.currentAp >= unit.maxAp)
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
                            unit.currentAp = actionPoints[unit];
                        }
                    }
                }
            }
        }
    }


    /// <summary>
    /// 해당 유닛의 턴을 실행합니다.
    /// </summary>
    private void ExecuteTurn(UnitSlotController unitSlot)
    {
        currentPrograssState = ProgressState.UnitPlay;

        //초기화
        currentTurnSlotNumber = unitSlotList.IndexOf(unitSlot);
        UnitBase currentTurnUnit = unitSlot.unit;
        SetCurrentUnitCardUI(true, currentTurnSlotNumber);
        SkillSlotInit(unitSlotList[currentTurnSlotNumber].unit.skillDataList);


        //현재 턴을 가진 유닛 구분
        SlotGroundSpriteController groundSprite = unitSlotList[currentTurnSlotNumber].slotGround;
        groundSprite.SetSlotGroundState(SlotGroundState.Select);
        turnUnitMarker.SetActive(true);
        turnUnitMarker.transform.parent = unitSlot.unit.hitPosition.transform;
        turnUnitMarker.transform.position = unitSlot.unit.hitPosition.transform.position;


        //액션 포인트 초기화
        actionPoints[currentTurnUnit] = 0;
        Debug.Log("턴을 시작합니다. 현재 턴은 " + unitSlotList[currentTurnSlotNumber].name + " (" + unitSlotList[currentTurnSlotNumber].unit.speed + " 속도) 유닛입니다.");
    }


    /// <summary>
    /// 턴을 종료합니다.
    /// </summary>
    public void TurnEnd()
    {
        //UnitGround의 색상 및 스프라이트, 애니메이션 초기화
        foreach (UnitSlotController unitSlot in unitSlotList)
        {
            if (unitSlot.isNull)
            {
                unitSlot.unit.SetAnim(0);
            }
            unitSlot.StatusInit();
            SlotGroundSpriteController groundSprite = unitSlot.slotGround;
            groundSprite.SetSlotGroundState(SlotGroundState.Default);
        }

        turnUnitMarker.SetActive(false);
        targetUnitMarker.SetActive(false);

        currentPrograssState = ProgressState.Stay;
    }
}
