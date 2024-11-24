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
        currentPrograssState = ProgressState.SkillPlay;
        //StartCoroutine(inGameManager.DelayTurnEnd(inGameManager.currentSkillSlot.skillData.skillEndTime));
        unitSlotList[currentTurnSlotNumber].unit.SetAnim(1);
        cost -= currentSkillSlot.skillData.skillCost;
    }

    public void SkillHitPlay()
    {
        SkillData skillData = currentSkillSlot.skillData;
        float skillDamage = skillData.damage;

        switch(skillData.skillTypeState)
        {
            case SkillTypeState.Attack:
                skillDamage += (unitSlotList[currentTurnSlotNumber].unit.atk);
                break;

            default:
                break;
        }
    }
}
