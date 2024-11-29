using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ������ �� ���¿� ���� ������ (Json�� ����� �������̸�, ���� ������ ����ϰ� �� ������)
/// </summary>
[System.Serializable]
public class UnitStatus
{
    [Header("Unit States")]
    public UnitData defaultUnitData;
    public int unitNumber = 0;
    public string unitName = "Null";
    public float hp = 10;
    public float ap = 10;
    public int atk = 1;
    public int def = 1;
    public float acc = 1;
    public float eva = 1;
    public int speed = 1;
    public List<int> skillNumberList; // skillDataList ��� ��ų�� �ĺ���(Number)�� ����Ͽ� ����

    public UnitStatus()
    {
        ApplyBaseStatus(defaultUnitData);
    }

    public UnitStatus(UnitData unitData)
    {
        unitNumber = unitData.unitNumber;
        ApplyBaseStatus(unitData);
    }

    /// <summary>
    /// �⺻ ���� �����͸� ����
    /// </summary>
    /// <param name="unitData">������ �� UnitData</param>
    public void ApplyBaseStatus(UnitData unitData)
    {
        if (unitData == null) return;

        unitNumber = unitData.unitNumber;
        unitName = unitData.unitName;
        hp = unitData.hp;
        ap = unitData.ap;
        atk = unitData.atk;
        def = unitData.def;
        acc = unitData.acc;
        eva = unitData.eva;
        speed = unitData.speed;

        // skillDataList�� �ִ� �� SkillData�� ID�� ����
        skillNumberList = new List<int>();
        foreach (SkillData skill in unitData.skillDataList)
        {
            skillNumberList.Add(skill.skillNumber);
        }
    }

    /// <summary>
    /// �߰� ���ݸ� ���� (�⺻ ���ݿ� �����ִ� ���)
    /// </summary>
    /// <param name="unitData">������ �� UnitData</param>
    public void ApplyPlusStatus(UnitData unitData)
    {
        if (unitData == null) return;

        unitNumber = unitData.unitNumber;
        unitName = unitData.unitName;

        // ���� ���� unitData�� ���� ������
        hp += unitData.hp;
        ap += unitData.ap;
        atk += unitData.atk;
        def += unitData.def;
        speed += unitData.speed;
        acc += unitData.acc;
        eva += unitData.eva;

        // skillDataList�� �ִ� �� SkillData�� Number(ID��)�� �߰� (�ߺ����� �ʵ��� ����)
        if (skillNumberList == null)
        {
            skillNumberList = new List<int>();
        }

        foreach (SkillData skill in unitData.skillDataList)
        {
            if (!skillNumberList.Contains(skill.skillNumber))
            {
                skillNumberList.Add(skill.skillNumber);
            }
        }
    }
}