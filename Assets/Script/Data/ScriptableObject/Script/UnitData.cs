using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 유닛의 테그
/// </summary>
[System.Serializable]
public enum UnitNative
{
    CounterSide,
    BlueAchive,
    Nikke
}

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
/// 유닛의 작품 중 역할 (파티 구성에 따라 스텟에 차이가 발생한다)
/// </summary>
[System.Serializable]
public enum UnitRole
{
    Main,
    Sub,
    Mob
}

/// <summary>
/// 수정 가능한 스텟의 State 값
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
/// 유닛의 가장 기초가되는 원본 데이터. (추후 게임 플레이를 통해 유닛을 강화하거나 업그레이드 할 수는 있으나, 가장 기본적인 데이터는 해당 데이터를 기준으로 작업한다.)
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
 