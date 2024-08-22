using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class GameManager
{


    /// <summary>
    /// ��ų ������ �ʱ�ȭ�ϰ� �־��� ��ų �����ͷ� �����մϴ�.
    /// </summary>
    /// <param name="setSkillDatas">������ ��ų ������ ����Ʈ�Դϴ�.</param>
    public void SkillSlotInit(List<SkillData> setSkillDatas)
    {
        for (int i = 0; i < setSkillDatas.Count; i++)
        {
            skillSlot[i].SetSkillData(setSkillDatas[i]);
        }
    }


    public void SkillStart()
    {
        currentPrograssState = ProgressState.SkillPlay;
        //StartCoroutine(inGameManager.DelayTurnEnd(inGameManager.currentSkillSlot.skillData.skillEndTime));
        unitSlots[currentTurnSlotNumber].SetAnim(1);
        cost -= currentSkillSlot.skillData.skillCost;
    }

    public void SkillHitPlay()
    {
        SkillData skillData = currentSkillSlot.skillData;
        float skillDamage = skillData.damage;

        switch(skillData.skillState)
        {
            case SkillState.Attack:
                skillDamage += (unitSlots[currentTurnSlotNumber].atk * skillData.skillCoefficient);
                break;

            default:
                break;
        }

        ExecuteAttack(skillDamage);
    }
}
