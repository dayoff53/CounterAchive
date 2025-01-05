using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 유닛의 현 상태에 대한 데이터 (Json과 UnitCard에 저장될 데이터, 실제 게임에 사용하게 될 데이터)
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
    public List<int> skillNumberList; // skillDataList 대신 스킬의 식별자(Number)를 저장하여 사용

    public UnitStatus()
    {
        if (unitData != null) // null 체크 추가
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
    /// UnitData의 데이터를 적용
    /// </summary>
    /// <param name="unitData">적용할 새 UnitData</param>
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

        // skillDataList에 있는 각 SkillData의 ID를 저장
        skillNumberList = new List<int>();
        foreach (SkillData skill in unitData.skillDataList)
        {
            skillNumberList.Add(skill.skillNumber);
        }
    }

    /// <summary>
    /// UnitBase의 데이터를 적용
    /// </summary>
    /// <param name="unitBase">적용할 새 UnitData</param>
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

        // skillDataList에 있는 각 SkillData의 ID를 저장
        skillNumberList = new List<int>();
        foreach (SkillData skill in unitBase.skillDataList)
        {
            skillNumberList.Add(skill.skillNumber);
        }
    }

    /// <summary>
    /// 추가 스탯을 적용 (기본 스탯에 더해지는 값)
    /// </summary>
    /// <param name="unitData">적용할 새 UnitData</param>
    public void ApplyPlusStatus(UnitData unitData)
    {
        if (unitData == null) return;

        unitNumber = unitData.unitNumber;
        unitName = unitData.unitName;

        // 현재 가진 unitData에 값을 더하기
        maxHp += unitData.hp;
        currentHp = maxHp;
        ap += unitData.ap;
        atk += unitData.atk;
        def += unitData.def;
        speed += unitData.speed;
        acc += unitData.acc;
        eva += unitData.eva;

        // skillDataList에 있는 각 SkillData의 Number(ID값)를 추가 (중복되지 않도록 체크)
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