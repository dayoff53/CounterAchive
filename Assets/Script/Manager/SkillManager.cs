using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : Singleton<SkillManager>
{
    [Tooltip("슬롯 메니저")]
    private UnitManager slotManager;
    [Tooltip("턴 메니저")]
    private TurnManager turnManager;
    [SerializeField]
    [Tooltip("스킬 슬롯 리스트")]
    private List<SkillSlotUIController> skillSlots;


    private Coroutine skillCoroutine;

    private void Start()
    {
        slotManager = UnitManager.Instance;
        turnManager = TurnManager.Instance;
    }

    public void SkillSlotsInit(List<SkillData> skillDatas)
    {
        for(int i = 0; i < skillDatas.Count; i++)
        {
            skillSlots[i].SetSkillData(skillDatas[i]);
        }
    }

    /// <summary>
    /// 지정된 범위와 데미지로 공격을 수행합니다. (추후 방어력이나 방어력 관통, 디버프등의 추가 데미지 공식을 넣을 예정)
    /// </summary>
    /// <param name="attackRange">공격할 범위입니다.</param>
    /// <param name="damage">적용할 데미지입니다.</param>
    public IEnumerator ExecuteSkill(SkillData skillData, UnitSlotController userUnit, List<UnitSlotController> targetUnits)
    {
        turnManager.turnState.SetState(new ActionTurnState());

        GameObject skillEffect;
        SkillEffectController effectController;


        userUnit.SetUnitState(UnitState.attack);
        yield return new WaitForSeconds(skillData.userSkillEffectTime);
        skillEffect = Instantiate(skillData.userSkillEffect, userUnit.transform.position, Quaternion.identity);
        effectController = skillEffect.GetComponent<SkillEffectController>();
        effectController.userUnit = userUnit;
        effectController.targetUnits = targetUnits;
        effectController.attackSelecter();
    }

    public void CancelSkillProcesses()
    {
        if (skillCoroutine != null)
            StopCoroutine(skillCoroutine);
    }
}
