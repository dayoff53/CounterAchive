using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class StageManager
{
    #region TurnVariable
    [Space(20)]
    [Header("------------------- Turn -------------------")]
    [Header("턴 데이터")]
    [Tooltip("유닛의 행동 포인트를 저장하는 Dictionary")]
    private SerializableDictionary<UnitBase, float> actionPoints = new SerializableDictionary<UnitBase, float>();

    /// <summary>
    /// 현재 턴의 슬롯 번호
    /// </summary>
    public int currentTurnSlotNumber;
    #endregion


    #region CostVariable
    [Space(20)]
    [Header("------------------- Cost -------------------")]
    [Header("Cost 변수")]
    private float _cost;
    public float cost 
    {
        get { return _cost; }
        set 
        {
            if(_cost != value)
            {
                _cost = value;
                UIManager.Instance.UpdateCostUI(_cost);
            }
        }
    }
    #endregion

    /// <summary>
    /// actionPoints의 현재 상태를 지속적으로 업데이트하는 코루틴
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
    /// ActionPoint를 누적하여 유닛의 행동 포인트를 증가시킵니다.
    /// </summary>
    /// <param name="time">시간 델타 값</param>
    private void ActionPointUpper(float time)
    {
        foreach (var unitSlot in unitSlotList)
        {
            if (unitSlot.unit != null && !unitSlot.isNull)
            {
                UnitBase unit = unitSlot.unit;
                if (actionPoints.TryGetValue(unit, out float currentPoints))
                {
                    actionPoints[unit] = currentPoints + unit.speed * time;
                    unit.currentAp = actionPoints[unit];

                    CostIncrease(unitSlot, time);

                    if (unit.currentAp >= unit.maxAp)
                    {
                        ExecuteTurn(unitSlot);
                        break;
                    }
                }
                else
                {
                    Debug.LogWarning($"키가 {unitSlot.name}에 존재하지 않습니다. 키를 추가합니다.");
                    actionPoints.Add(unit, unit.speed * time);  // 유닛의 행동 포인트 초기화
                    unit.currentAp = actionPoints[unit];
                }
            }
        }
    }

    /// <summary>
    /// 해당 유닛 슬롯의 비용을 증가시킵니다.
    /// </summary>
    /// <param name="unitSlot">비용을 증가시킬 유닛 슬롯</param>
    /// <param name="time">시간 델타 값</param>
    private void CostIncrease(UnitSlotController unitSlot, float time)
    {
        if (unitSlot != null)
        {
            if (unitSlot.unitTeam == 1)
            {
                float currentSpeed = unitSlot.unit.speed;

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

    /// <summary>
    /// 턴을 건너뛰는 메서드
    /// </summary>
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
                    if (unitSlot != null && !unitSlot.isNull)
                    {
                        UnitBase unit = unitSlot.unit;
                        if (actionPoints.TryGetValue(unit, out float currentPoints))
                        {
                            actionPoints[unit] = currentPoints + unit.speed * skipTime;
                            unit.currentAp = actionPoints[unit];

                            CostIncrease(unitSlot, skipTime);

                            if (unit.currentAp >= unit.maxAp)
                            {
                                isSkip = false;
                                ExecuteTurn(unitSlot);
                                break;
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"키가 {unit.name}에 존재하지 않습니다. 키를 추가합니다.");
                            actionPoints.Add(unit, unit.speed * Time.deltaTime);  // 유닛의 행동 포인트 초기화
                            unit.currentAp = actionPoints[unit];
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 선택된 유닛의 턴을 실행합니다.
    /// </summary>
    /// <param name="unitSlot">턴을 실행할 유닛 슬롯</param>
    private void ExecuteTurn(UnitSlotController unitSlot)
    {
        currentPrograssState = ProgressState.UnitPlay;

        // 초기화
        currentTurnSlotNumber = unitSlotList.IndexOf(unitSlot);
        UnitBase currentTurnUnit = unitSlot.unit;
        SetCurrentUnitCardUI(true, currentTurnSlotNumber);
        SkillSlotInit(unitSlotList[currentTurnSlotNumber].unit.skillDataList);

        // 슬롯 상태 업데이트
        SlotGroundSpriteController groundSprite = unitSlotList[currentTurnSlotNumber].slotGround;
        groundSprite.SetSlotGroundState(SlotGroundState.Select);
        currentTurnUnit.SetTurn(true);


        // 초기화
        actionPoints[currentTurnUnit] = 0;
        Debug.Log($"턴이 시작되었습니다. 현재 유닛: {unitSlotList[currentTurnSlotNumber].name} (속도: {unitSlotList[currentTurnSlotNumber].unit.speed})");
    }

    /// <summary>
    /// 턴을 종료합니다.
    /// </summary>
    public void TurnEnd()
    {
        // 유닛 사망 여부 확인
            // 모든 유닛의 애니메이션 및 슬롯 상태 초기화
            foreach (UnitSlotController unitSlot in unitSlotList)
            {
                unitSlot.unit.SetAnim(0);
                unitSlot.TurnEndInit();
                SlotGroundSpriteController groundSprite = unitSlot.slotGround;
                groundSprite.SetSlotGroundState(SlotGroundState.Default);
            }

        unitSlotList[currentTurnSlotNumber].unit.SetTurn(false);
        currentTurnCount++;
        UpdateStageClearCondition();
    }
}
