using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 유닛의 현 상태에 관한 데이터 (Json과 UnitCard에 저장될 데이터, 유닛 생성시 사용하게 될 데이터)
/// </summary>
[System.Serializable]
public class UnitStatus
{
    [Header("Unit States")]
    public UnitData defaultUnitData;
    public int unitNumber = 0;
    public string unitName = "Null";
    public float maxHp = 10;
    public float currentHp = 10;
    public float ap = 10;
    public float atk = 1;
    public float def = 1;
    public float acc = 1;
    public float eva = 1;
    public float speed = 1;
    public List<int> skillNumberList; // skillDataList 대신 스킬의 식별자(Number)를 사용하여 저장

    public UnitStatus()
    {
        if (defaultUnitData != null) // null 체크 추가
        {
            SetStatus(defaultUnitData);
        }
    }

    public UnitStatus(UnitData unitData)
    {
        unitNumber = unitData.unitNumber;
        SetStatus(unitData);
    }

    /// <summary>
    /// UnitData의 데이터를 적용
    /// </summary>
    /// <param name="unitData">기준이 될 UnitData</param>
    public void SetStatus(UnitData unitData)
    {
        if (unitData == null) return;

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

        // skillDataList에 있는 각 SkillData의 ID만 저장
        skillNumberList = new List<int>();
        foreach (SkillData skill in unitData.skillDataList)
        {
            skillNumberList.Add(skill.skillNumber);
        }
    }

    /// <summary>
    /// UnitBase의 데이터를 적용
    /// </summary>
    /// <param name="unitBase">기준이 될 UnitData</param>
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

        // skillDataList에 있는 각 SkillData의 ID만 저장
        skillNumberList = new List<int>();
        foreach (SkillData skill in unitBase.skillDataList)
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
        maxHp += unitData.hp;
        currentHp = maxHp;
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