using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class StageManager
{
    #region SkillVariable
    [Space(20)]
    [Header("------------------- Skill -------------------")]
    [Header("스킬 기본 변수")]
    /// <summary>
    /// 현재 선택된 스킬의 타겟 슬롯 리스트
    /// </summary>
    public List<UnitSlotController> currentSkillTargetSlots;
    
    /// <summary>
    /// 현재 선택된 스킬 데이터
    /// </summary>
    private SkillData _currentSkillData;
    public SkillData currentSkillData
    {
        get { return _currentSkillData; }
        set
        {
            _currentSkillData = value;
            stageMenuController.StageMenuInit();
        }
    }

    /// <summary>
    /// 스킬의 성공여부를 판정하는 수치 (0~1)
    /// </summary>
    private float skillAcc = 0;

    /// <summary>
    /// 스킬의 판정 성공 여부 (Acc를 기준으로 함)
    /// </summary>
    private bool isSkillSuccess = false;

    /// <summary>
    /// 스킬의 연출 발생 횟수
    /// </summary>
    private int skillHitProductionCount = 0;
    #endregion

    #region SkillSlotVariable
    [Space(10)]
    [Header("스킬 슬롯 변수")]
    [SerializeField]
    [Tooltip("스킬 슬롯 리스트")]
    /// <summary>
    /// 스킬 데이터를 플레이어에 보여주는 UI 슬롯 리스트
    /// </summary>
    public List<SkillSlotUIController> skillSlotList;

    /// <summary>
    /// 현재 선택된 스킬 타겟의 슬롯 번호
    /// </summary>
    private int _skillTargetNum;
    public int skillTargetNum
    {
        get { return _skillTargetNum; }
        set
        {
            _skillTargetNum = value;

            if (UIManager.Instance.targetUnitCardUI != null && skillTargetNum >= 0)
            {
                UIManager.Instance.UpdateUnitCardUI(false, unitSlotList[skillTargetNum].unit);
            }
        }
    }
    #endregion


    /// <summary>
    /// 스킬 슬롯을 초기화하고 선택된 스킬 데이터를 슬롯에 할당합니다.
    /// </summary>
    /// <param name="setSkillDataList">초기화할 스킬 데이터 리스트입니다.</param>
    public void SkillSlotInit(List<SkillData> setSkillDataList)
    {
        skillTargetNum = -1;

        for (int i = 0; i < skillSlotList.Count; i++)
        {
            skillSlotList[i].SetSkillData(null);
        }

        for (int i = 0; i < setSkillDataList.Count; i++)
        {
            skillSlotList[i].SetSkillData(setSkillDataList[i]);
        }
    }

    /// <summary>
    /// 스킬 타입을 선택하고 해당 스킬의 범위를 표시합니다.
    /// </summary>
    /// <param name="skillData">선택된 스킬 데이터입니다.</param>
    public void SkillTypeSelect(SkillData skillData)
    {
        if (skillData != null && (currentPrograssState == ProgressState.UnitPlay || currentPrograssState == ProgressState.SkillTargetSearch))
        {
            currentPrograssState = ProgressState.SkillTargetSearch;
            SetRangeGround(skillData.skillRange);
            currentSkillData = skillData;
        }
    }

    /// <summary>
    /// 스킬의 타겟 슬롯을 선택하고 필요한 정보를 확인합니다.
    /// </summary>
    /// <param name="selectTargetSlot">선택된 타겟 슬롯 컨트롤러입니다.</param>
    public void SkillTargetSelect(UnitSlotController selectTargetSlot)
    {
        if (cost >= currentSkillData.skillCost)
        {
            if (skillTargetNum == unitSlotList.IndexOf(selectTargetSlot))
            {
                SkillStart();
            }

            skillTargetNum = unitSlotList.IndexOf(selectTargetSlot);
            SetCurrentUnitCardUI(false, skillTargetNum);

            if (currentTurnSlotNumber <= skillTargetNum)
                unitSlotList[currentTurnSlotNumber].unit.SetDirection(false);
            else
                unitSlotList[currentTurnSlotNumber].unit.SetDirection(true);

            skillAcc = ((unitSlotList[currentTurnSlotNumber].unit.acc * (currentSkillData.skillAcc * 0.01f)) / unitSlotList[skillTargetNum].unit.eva);
            skillAccuracyText.text = $"{skillAcc * 100}%";

            skillHitProductionCount = currentSkillData.skillHitCount;

            targetUnitMarker.SetActive(true);
            targetUnitMarker.transform.parent = selectTargetSlot.unit.productionPositionList[0].transform;
            targetUnitMarker.transform.position = selectTargetSlot.unit.productionPositionList[0].transform.position;


            Debug.Log($"stageManager.skillTargetNum = {skillTargetNum}");
        }
        else
        {
            SetCurrentUnitCardUI(false, 0);
            Debug.Log("코스트가 부족합니다.");
        }
    }

    /// <summary>
    /// 스킬을 시작하고 명중 여부를 판단합니다. 
    /// (현재 진행 상태가 SkillPlay로 전환되고, 유닛 애니메이션이 시작됩니다.)
    /// </summary>
    public void SkillStart()
    {
        if (currentSkillData)
        {
            isSkillSuccess = false;
            float randomValue = Random.value;
            if (randomValue < skillAcc)
            {
                isSkillSuccess = true;
                Debug.Log($"{currentSkillData.skillName} 스킬이 성공적으로 발동되었습니다.");
            } else
            {
                Debug.Log($"{currentSkillData.skillName} 스킬이 실패하였습니다.");
            }

            // 스킬 효과를 적용합니다 (SkillEndPlay 호출)
            currentSkillTargetSlots = new List<UnitSlotController>();
            foreach (int skillAreaNum in currentSkillData.skillArea)
            {
                if (unitSlotList.IndexOf(unitSlotList[skillTargetNum + skillAreaNum]) != -1)
                {
                    currentSkillTargetSlots.Add(unitSlotList[skillTargetNum + skillAreaNum]);
                }
            }

            cost -= currentSkillData.skillCost;
            currentPrograssState = ProgressState.SkillPlay;
            unitSlotList[currentTurnSlotNumber].unit.SetAnim(1);

            targetUnitMarker.SetActive(false);
        }
    }

    /// <summary>
    /// 스킬의 연출 효과를 적용합니다.
    /// </summary>
    public virtual void SkillProduction(int hitProductionNum)
    {
        foreach (UnitSlotController targetUnit in currentSkillTargetSlots)
        {
            Debug.Log($"{targetUnit}의 SkillProduction 호출");
            if(!currentSkillData.isSkillHitMultiple)
            {
                GameObject hitProductonObject = poolManager.Pop(currentSkillData.skillHitProductionObjects[hitProductionNum]);
                targetUnit.unit.HitProduction(hitProductonObject, currentSkillData.skillHitRadius);
                targetUnit.unit.SetAnim(2);
            } else if (currentSkillData.isSkillHitMultiple && skillHitProductionCount > 0)
            {
                GameObject hitProductonObject = poolManager.Pop(currentSkillData.skillHitProductionObjects[hitProductionNum]);
                targetUnit.unit.HitProduction(hitProductonObject, currentSkillData.skillHitRadius);
                targetUnit.unit.SetAnim(2);
                skillHitProductionCount--;
            }
        }
    }



    /// <summary>
    /// 스킬이 발동 종료 이후, 스킬 효과를 적용합니다.
    /// </summary>
    public void SkillEndPlay()
    {
        if (isSkillSuccess)
        {
            foreach (SkillEffect skilleffect in currentSkillData.skillEffectList)
            {
                switch (skilleffect.skillEffectState)
                {
                    case SkillEffectState.Damage:
                        foreach (UnitSlotController targetUnit in currentSkillTargetSlots)
                            targetUnit.unit.Damage(skilleffect.valueList[0] * unitSlotList[currentTurnSlotNumber].unit.atk);
                        break;

                    case SkillEffectState.StatusDown:
                        foreach (UnitSlotController targetUnit in currentSkillTargetSlots)
                            targetUnit.unit.currentHp -= skilleffect.valueList[0];
                        break;

                    default:
                        break;
                }
            }
        }
    }


    /// <summary>
    /// 스킬의 범위에 따라 그라운드 상태를 설정합니다.
    /// </summary>
    /// <param name="skillRange">스킬의 범위 배열입니다.</param>
    public void SetRangeGround(int[] skillRange)
    {
        SlotGroundSpriteController groundSprite;

        for (int i = 0; i < unitSlotList.Count; i++)
        {
            groundSprite = unitSlotList[i].slotGround;

            groundSprite.SetSlotGroundState(SlotGroundState.Default);
        }

        for (int i = 0; i < skillRange.Length; i++)
        {

            if (currentTurnSlotNumber + skillRange[i] < unitSlotList.Count)
            {
                groundSprite = unitSlotList[currentTurnSlotNumber + skillRange[i]].slotGround;

                groundSprite.SetSlotGroundState(SlotGroundState.Target);
            }

            if (currentTurnSlotNumber - skillRange[i] >= 0)
            {
                groundSprite = unitSlotList[currentTurnSlotNumber - skillRange[i]].slotGround;

                groundSprite.SetSlotGroundState(SlotGroundState.Target);
            }
        }

        groundSprite = unitSlotList[currentTurnSlotNumber].slotGround;
        groundSprite.SetSlotGroundState(SlotGroundState.Select);
    }
}
