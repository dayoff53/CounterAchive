using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class StageManager
{

    /// <summary>
    /// ��ų ������ �ʱ�ȭ�ϰ� �־��� ��ų �����ͷ� �����մϴ�.
    /// </summary>
    /// <param name="setSkillDataList">������ ��ų ������ ����Ʈ�Դϴ�.</param>
    public void SkillSlotInit(List<SkillData> setSkillDataList)
    {
        for (int i = 0; i < setSkillDataList.Count; i++)
        {
            skillSlotList[i].SetSkillData(setSkillDataList[i]);
        }
    }


    public void SkillStart()
    {
        SkillData skillData = currentSkillSlot.skillData;
        currentPrograssState = ProgressState.SkillPlay;
        //StartCoroutine(inGameManager.DelayTurnEnd(inGameManager.currentSkillSlot.skillData.skillEndTime));
        unitSlotList[currentTurnSlotNumber].unit.SetAnim(1);
        cost -= skillData.skillCost;


        NormalAttack(skillData, unitSlotList[currentTurnSlotNumber].unit, unitSlotList[skillTargetNum].unit);
    }

    public void SkillHitPlay()
    {
        SkillData skillData = currentSkillSlot.skillData;

        switch(skillData.skillTypeState)
        {
            case SkillTypeState.Attack:
                HitProduction();
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// �� ��ǥ���� ������ �����մϴ�.
    /// </summary>
    /// <param name="damage">������ �������Դϴ�.</param>
    public void ExecuteAttack(UnitBase target, float damage)
    {
        target.currentHp -= damage;
    }

    public virtual void HitProduction()
    {
        GameObject hitProductonObject = poolManager.Pop(currentSkillSlot.skillData.skilIHitProductionObject);
        hitProductonObject.transform.position = unitSlotList[skillTargetNum].transform.position;
    }


    protected virtual void NormalAttack(SkillData skillData, UnitBase user, UnitBase target)
    {
        float skillDamage = skillData.damage;

        switch (skillData.skillTypeState)
        {
            case SkillTypeState.Attack:
                skillDamage += (user.atk);
                ExecuteAttack(target, skillDamage);
                break;

            default:
                break;
        }
    }
}
