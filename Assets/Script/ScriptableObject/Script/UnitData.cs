using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
    public float hp;
    public int atk;
    public int speed;
    public float actionPoint = 100;
    public List<SkillData> skillDataList;

    /* UnitState�� ����Ͽ� ���̻��� �ʿ� ���� ��ũ��Ʈ
    /// <summary>
    /// UnitStats�� ���纻�� �����Ͽ� ��ȯ�մϴ�.
    /// </summary>
    public UnitData CreateCopy()
    {
        // ���ο� UnitStats �ν��Ͻ� ����
        UnitData copy = ScriptableObject.CreateInstance<UnitData>();

        // ��� �ʵ� ���� ����
        copy.unitNumber = this.unitNumber;
        copy.unitName = this.unitName;
        copy.unitFaceIcon = this.unitFaceIcon;
        copy.unitSprite = this.unitSprite;
        copy.unitAnimController = this.unitAnimController;
        copy.hp = this.hp;
        copy.atk = this.atk;
        copy.speed = this.speed;
        copy.actionPoint = this.actionPoint;

        // SkillData ����Ʈ�� �����̹Ƿ� ���ο� ����Ʈ�� �����Ͽ� ����
        copy.skillDataList = new List<SkillData>();
        foreach (SkillData skill in skillDataList)
        {
            // SkillData�� �������̹Ƿ� ������ ���� ������ �ʿ��� ��� �߰��ؾ� ��
            copy.skillDataList.Add(skill.CreateCopy());
        }

        return copy;
    }
    */
}
