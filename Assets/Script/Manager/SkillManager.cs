using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : Singleton<SkillManager>
{
    [Tooltip("���� �޴���")]
    private UnitManager slotManager;
    [Tooltip("�� �޴���")]
    private TurnManager turnManager;
    [SerializeField]
    [Tooltip("��ų ���� ����Ʈ")]
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
    /// ������ ������ �������� ������ �����մϴ�. (���� �����̳� ���� ����, ��������� �߰� ������ ������ ���� ����)
    /// </summary>
    /// <param name="attackRange">������ �����Դϴ�.</param>
    /// <param name="damage">������ �������Դϴ�.</param>
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
