using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ������ �ױ�
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
/// ������ ��ǰ �� ���� (��Ƽ ������ ���� ���ݿ� ���̰� �߻��Ѵ�)
/// </summary>
[System.Serializable]
public enum UnitRole
{
    Main,
    Sub,
    Mob
}

/// <summary>
/// ���� ������ ������ State ��
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
/// ������ ���� ���ʰ��Ǵ� ���� ������. (���� ���� �÷��̸� ���� ������ ��ȭ�ϰų� ���׷��̵� �� ���� ������, ���� �⺻���� �����ʹ� �ش� �����͸� �������� �۾��Ѵ�.)
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
 