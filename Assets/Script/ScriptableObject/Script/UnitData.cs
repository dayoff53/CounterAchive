using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New UnitData", menuName = "UnitData")]
public class UnitData : ScriptableObject
{
    [Header("Unit Stats")]
    public string unitName;
    public Sprite unitFaceIcon;
    public Sprite unitSprite;
    public int hp;
    public int atk;
    public int speed;
    public float actionPoint = 100;
    public List<SkillData> skillDatas;
}
