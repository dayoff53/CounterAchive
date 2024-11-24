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
    /// 스킬 슬롯을 초기화하고 주어진 스킬 데이터로 설정합니다.
    /// </summary>
    /// <param name="setSkillDataList">설정할 스킬 데이터 리스트입니다.</param>
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
