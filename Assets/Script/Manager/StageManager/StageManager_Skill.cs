using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class StageManager
{
    public List<UnitSlotController> currentSkillTargetSlots;
    private SkillData _currentSkillData;
    public SkillData currentSkillData
    {
        get
        {
            return _currentSkillData;
        }
        set
        {
            _currentSkillData = value;

            stageMenuController.StageMenuInit();
        }
    }
    private float skillAcc = 0;
    private bool isSkillSuccess = false;
    private SkillRangeUIController skillRangeUIController;
    public StageMenuController stageMenuController;


    /// <summary>
    /// 스킬 슬롯을 초기화하고 설정된 스킬 데이터를 슬롯에 할당합니다.
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
    /// 스킬 유형을 선택하고 해당 스킬의 범위를 설정합니다.
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
    /// 스킬의 타겟 슬롯을 선택하고 필요한 조건을 확인합니다.
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

            targetUnitMarker.SetActive(true);
            targetUnitMarker.transform.parent = selectTargetSlot.unit.hitPosition.transform;
            targetUnitMarker.transform.position = selectTargetSlot.unit.hitPosition.transform.position;


            Debug.Log($"stageManager.skillTargetNum = {skillTargetNum}");
        }
        else
        {
            SetCurrentUnitCardUI(false, 0);
            Debug.Log("비용이 부족합니다.");
        }
    }

    /// <summary>
    /// 스킬을 시작하고 성공 여부를 판단합니다. 
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
    /// 스킬이 종료되었을 때 스킬 효과를 적용합니다.
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
    /// 스킬의 타격 효과를 생성합니다.
    /// </summary>
    public virtual void SkillProduction(int hitProductionNum)
    {
        foreach (UnitSlotController targetUnit in currentSkillTargetSlots)
        {
            Debug.Log($"{targetUnit}의 SkillProduction 호출");
            GameObject hitProductonObject = poolManager.Pop(currentSkillData.skillHitProductionObjects[hitProductionNum]);
            targetUnit.unit.HitProduction(hitProductonObject, currentSkillData.skillHitRadius);
            targetUnit.unit.SetAnim(2);
        }
    }


    /// <summary>
    /// 스킬의 히트 효과를 생성합니다.
    /// </summary>
    public virtual void HitProduction(int hitProductionNum)
    {
        foreach (UnitSlotController targetUnit in currentSkillTargetSlots)
        {
            if (!targetUnit.isNull)
            {
                targetUnit.unit.SetAnim(2);
            }
        }
        GameObject hitProductonObject = poolManager.Pop(currentSkillData.skillHitProductionObjects[hitProductionNum]);

        Vector3 hitPos = unitSlotList[skillTargetNum].unit.hitPosition.gameObject.transform.position;

        if (currentSkillData.skillHitRadius != 0)
        {
            float angle = Random.Range(0, 360);

            float randomRadius = Random.Range(0f, currentSkillData.skillHitRadius);

            float x = hitPos.x + randomRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = hitPos.y + randomRadius * Mathf.Sin(angle * Mathf.Deg2Rad);

            hitProductonObject.transform.position = new Vector3(x, y, hitPos.z);
        }
    }



    /// <summary>
    /// 스킬의 범위에 따라 그리드 상태를 설정합니다.
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
