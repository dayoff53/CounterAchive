using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 유닛의 소속
/// </summary>
[System.Serializable]
public enum UnitTag_Native
{
    CounterSide,
    BlueAchive,
    Nikke
}

/// <summary>
/// 유닛의 태그
/// </summary>
public enum UnitTag
{
    #region Gender
    Male,
    Female,
    #endregion
    #region Species
    Human,
    Beast,
    #endregion
    #region Age
    Kid,
    Student,
    Youth,
    Elder,
    Longevous,
    Immortal
    #endregion
}


/// <summary>
/// 유닛의 상품 및 등급 (파티 구성시 등급 제한에 걸리게 발생한다)
/// </summary>
[System.Serializable]
public enum UnitRole
{
    Main,
    Sub,
    Mob
}

/// <summary>
/// 공개 유닛의 스테이터스 State 값
/// </summary>
[System.Serializable]
public enum PublicUnitStatusState
{
    maxHp,
    currentHp,
    maxAp,
    atk,
    def,
    speed,
    acc,
    eva,
    currentActionPoint
}


/// <summary>
/// 유닛의 기본 기초가되는 기본 데이터. (실제 게임 플레이를 통해 데이터가 변화하거나 업그레이드 된 유닛 데이터, 실제 기본적인 데이터는 해당 데이터를 기반으로 작업한다.)
/// </summary>
[CreateAssetMenu(fileName = "New UnitData", menuName = "Datas/UnitData")]
public class UnitData : ScriptableObject
{
    [Header("Unit Stats")]
    public int unitNumber = 0;
    public string unitName;
    public Sprite unitFaceIcon;
    public Sprite unitSprite;
    public RuntimeAnimatorController unitAnimController;
    public Vector3 unitPosition = new Vector3(0, 2.175f, 0);
    public Vector3 hitPosition;

    public float hp = 10;
    public float ap = 10;
    public int atk = 1;
    public int def = 1;
    public int speed = 1;
    public float acc = 100;
    public float eva = 5;
    public List<UnitTag> unitTagList;
    public List<SkillData> skillDataList; 
}