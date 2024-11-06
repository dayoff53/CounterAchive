using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
    public float hp;
    public int atk;
    public int speed;
    public float actionPoint = 100;
    public List<SkillData> skillDataList;

    /* UnitState를 사용하여 더이상은 필요 없는 스크립트
    /// <summary>
    /// UnitStats의 복사본을 생성하여 반환합니다.
    /// </summary>
    public UnitData CreateCopy()
    {
        // 새로운 UnitStats 인스턴스 생성
        UnitData copy = ScriptableObject.CreateInstance<UnitData>();

        // 모든 필드 값을 복사
        copy.unitNumber = this.unitNumber;
        copy.unitName = this.unitName;
        copy.unitFaceIcon = this.unitFaceIcon;
        copy.unitSprite = this.unitSprite;
        copy.unitAnimController = this.unitAnimController;
        copy.hp = this.hp;
        copy.atk = this.atk;
        copy.speed = this.speed;
        copy.actionPoint = this.actionPoint;

        // SkillData 리스트는 참조이므로 새로운 리스트를 생성하여 복사
        copy.skillDataList = new List<SkillData>();
        foreach (SkillData skill in skillDataList)
        {
            // SkillData도 참조형이므로 별도의 복사 로직이 필요할 경우 추가해야 함
            copy.skillDataList.Add(skill.CreateCopy());
        }

        return copy;
    }
    */
}
