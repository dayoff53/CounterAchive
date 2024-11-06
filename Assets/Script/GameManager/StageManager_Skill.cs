using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class StageManager
{
    private DataManager dataManager;

    private void Start()
    {
        dataManager = DataManager.Instance;
    }

    /// <summary>
    /// ��ų ������ �ʱ�ȭ�ϰ� �־��� ��ų �����ͷ� �����մϴ�.
    /// </summary>
    /// <param name="setSkillNumberList">������ ��ų ������ ����Ʈ�Դϴ�.</param>
    public void SkillSlotInit(List<int> setSkillNumberList)
    {
        for (int i = 0; i < setSkillNumberList.Count; i++)
        {
            skillSlotList[i].SetSkillData(dataManager.skillList.Find(skill => skill.skillNumber == setSkillNumberList[i]));
        }
    }


    public void SkillStart()
    {
        currentPrograssState = ProgressState.SkillPlay;
        //StartCoroutine(inGameManager.DelayTurnEnd(inGameManager.currentSkillSlot.skillData.skillEndTime));
        unitSlotList[currentTurnSlotNumber].SetAnim(1);
        cost -= currentSkillSlot.skillData.skillCost;
    }

    public void SkillHitPlay()
    {
        SkillData skillData = currentSkillSlot.skillData;
        float skillDamage = skillData.damage;

        switch(skillData.skillState)
        {
            case SkillState.Attack:
                skillDamage += (unitSlotList[currentTurnSlotNumber].atk * skillData.skillCoefficient);
                break;

            default:
                break;
        }

        ExecuteAttack(skillDamage);
    }
}
