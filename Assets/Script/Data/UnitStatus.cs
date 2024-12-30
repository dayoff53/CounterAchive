using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ������ �� ���¿� ���� ������ (Json�� UnitCard�� ����� ������, ���� ������ ����ϰ� �� ������)
/// </summary>
[System.Serializable]
public class UnitStatus
{
    [Header("Unit States")]
    public bool isOriginal = true;
    public UnitData unitData;
    public int unitNumber = 0;
    public string unitName = "";
    public float maxHp = 10;
    public float currentHp = 10;
    public float ap = 10;
    public float atk = 1;
    public float def = 1;
    public float acc = 1;
    public float eva = 1;
    public float speed = 1;
    public List<UnitTag> unitTagList;
    public List<int> skillNumberList; // skillDataList ��� ��ų�� �ĺ���(Number)�� ����Ͽ� ����

    public UnitStatus()
    {
        if (unitData != null) // null üũ �߰�
        {
            SetStatus(unitData);
        }
    }

    public UnitStatus(UnitData unitData)
    {
        unitNumber = unitData.unitNumber;
        SetStatus(unitData);
    }

    /// <summary>
    /// UnitData�� �����͸� ����
    /// </summary>
    /// <param name="unitData">������ �� UnitData</param>
    public void SetStatus(UnitData unitData)
    {
        if (unitData == null) return;

        this.unitData = unitData;
        unitNumber = unitData.unitNumber;
        unitName = unitData.unitName;
        maxHp = unitData.hp;
        currentHp = maxHp;
        ap = unitData.ap;
        atk = unitData.atk;
        def = unitData.def;
        acc = unitData.acc;
        eva = unitData.eva;
        speed = unitData.speed;
        unitTagList = unitData.unitTagList;

        // skillDataList�� �ִ� �� SkillData�� ID�� ����
        skillNumberList = new List<int>();
        foreach (SkillData skill in unitData.skillDataList)
        {
            skillNumberList.Add(skill.skillNumber);
        }
    }

    /// <summary>
    /// UnitBase�� �����͸� ����
    /// </summary>
    /// <param name="unitBase">������ �� UnitData</param>
    public void SetStatus(UnitBase unitBase)
    {
        if (unitBase == null) return;

        unitNumber = unitBase.unitData.unitNumber;
        unitName = unitBase.unitName;
        maxHp = unitBase.maxHp;
        currentHp = unitBase.currentHp;
        ap = unitBase.maxAp;
        atk = unitBase.atk;
        def = unitBase.def;
        acc = unitBase.acc;
        eva = unitBase.eva;
        speed = unitBase.speed;
        unitTagList = unitBase.unitTagList;

        // skillDataList�� �ִ� �� SkillData�� ID�� ����
        skillNumberList = new List<int>();
        foreach (SkillData skill in unitBase.skillDataList)
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
        maxHp += unitData.hp;
        currentHp = maxHp;
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