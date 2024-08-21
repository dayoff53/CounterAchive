using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SkillManager : Singleton<SkillManager>
{
    private DataManager dataManager;
    private UnitSlotController targetUnitSlot;

    [SerializeField]
    [Header("SkillSlot")]
    [Tooltip("스킬 슬롯 리스트")]
    public List<SkillSlotUIController> skillSlot;

    public int skillTargetNum;

    public SkillSlotUIController currentSkillSlot;


    void Start()
    {
        dataManager = DataManager.Instance;
    }



    /// <summary>
    /// 스킬 슬롯을 초기화하고 주어진 스킬 데이터로 설정합니다.
    /// </summary>
    /// <param name="setSkillDatas">설정할 스킬 데이터 리스트입니다.</param>
    public void SkillSlotInit(List<SkillData> setSkillDatas)
    {
        for (int i = 0; i < setSkillDatas.Count; i++)
        {
            skillSlot[i].SetSkillData(setSkillDatas[i]);
        }
    }


    public void SkillStart()
    {
        dataManager.currentTurn = turnState.SkillPlay;
        //StartCoroutine(inGameManager.DelayTurnEnd(inGameManager.currentSkillSlot.skillData.skillEndTime));
        dataManager.unitSlots[dataManager.currentTurnSlotNumber].SetAnim(1);
        dataManager.cost -= currentSkillSlot.skillData.skillCost;
    }

    public void SkillHitPlay()
    {
        SkillData skillData = currentSkillSlot.skillData;
        float skillDamage = skillData.damage;

        switch(skillData.skillState)
        {
            case SkillState.Attack:
                skillDamage += (dataManager.unitSlots[dataManager.currentTurnSlotNumber].atk * skillData.skillCoefficient);
                break;

            default:
                break;
        }

        dataManager.ExecuteAttack(skillDamage);
    }
}
