using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 유닛의 현 상태에 관한 데이터 (Json에 저장될 데이터이며, 유닛 생성시 사용하게 될 데이터)
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
    public List<int> skillNumberList; // skillDataList 대신 스킬의 식별자(Number)를 사용하여 저장

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
    /// 기본 유닛 데이터를 적용
    /// </summary>
    /// <param name="unitData">기준이 될 UnitData</param>
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

        // skillDataList에 있는 각 SkillData의 ID만 저장
        skillNumberList = new List<int>();
        foreach (SkillData skill in unitData.skillDataList)
        {
            skillNumberList.Add(skill.skillNumber);
        }
    }

    /// <summary>
    /// 추가 스텟를 적용 (기본 스텟에 더해주는 방식)
    /// </summary>
    /// <param name="unitData">기준이 될 UnitData</param>
    public void ApplyPlusStatus(UnitData unitData)
    {
        if (unitData == null) return;

        unitNumber = unitData.unitNumber;
        unitName = unitData.unitName;

        // 기존 값에 unitData의 값을 더해줌
        hp += unitData.hp;
        ap += unitData.ap;
        atk += unitData.atk;
        def += unitData.def;
        speed += unitData.speed;
        acc += unitData.acc;
        eva += unitData.eva;

        // skillDataList에 있는 각 SkillData의 Number(ID값)만 추가 (중복되지 않도록 관리)
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