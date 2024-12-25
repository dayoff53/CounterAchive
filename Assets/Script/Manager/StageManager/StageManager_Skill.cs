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
    /// ��ų ������ �ʱ�ȭ�ϰ� �־��� ��ų �����ͷ� �����մϴ�.
    /// </summary>
    /// <param name="setSkillDataList">������ ��ų ������ ����Ʈ�Դϴ�.</param>
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
    /// ���õ� ��ų�� �����͸� UI�� �����մϴ�.
    /// </summary>
    /// <param name="skillData"></param>
    public void SkillTypeSelect(SkillData skillData)
    {
        if (skillData != null)
        {
            currentPrograssState = ProgressState.SkillTargetSearch;
            SetRangeGround(skillData.skillRange);
            currentSkillData = skillData;
        }
    }

    /// <summary>
    /// ��ų�� ��ǥ �����͸� UI�� �����մϴ�.
    /// </summary>
    /// <param name="selectTargetSlot"></param>
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
            Debug.Log("Not enough costs");
        }
    }

    /// <summary>
    /// ��ų�� �ʱ� ������ �����մϴ�. (PrograssState�� SkillPlay���� �����ϰų�, Unit�� Anim�� �۵� ��Ű�µ��� �۾�)
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
                Debug.Log($"{currentSkillData.skillName}�� ��ų ���� ����");
            } else
            {
                Debug.Log($"{currentSkillData.skillName}�� ��ų ���� ����");
            }

            //��ų ȿ��(SkillEndPlay)�� ���� ������ �����
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
    /// ��ų�� ���� �� ���� �ش� ��ų�� ����� ������� �����Ų��.
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
    /// ��ų�� ������ ����ϴ� �޼���
    /// </summary>
    public virtual void SkillProduction(int hitProductionNum)
    {
        foreach (UnitSlotController targetUnit in currentSkillTargetSlots)
        {
            Debug.Log($"{targetUnit}�� SkillProduction");
            GameObject hitProductonObject = poolManager.Pop(currentSkillData.skillHitProductionObjects[hitProductionNum]);
            targetUnit.unit.HitProduction(hitProductonObject, currentSkillData.skillHitRadius);
            targetUnit.unit.SetAnim(2);
        }
    }


    /// <summary>
    /// ��ų�� ������ ����ϴ� �޼���
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
    /// ��ų ��Ÿ���ŭ groundSprite�� ����
    /// </summary>
    /// <param name="skillRange"></param>
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
