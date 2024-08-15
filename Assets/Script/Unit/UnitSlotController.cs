using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UnitState
{
    normal,
    attack,
    hit,
    stun,
    death
}

public class UnitSlotController : MonoBehaviour
{
    [SerializeField]
    [Header("SlotNum")]
    public int slotNum = 0;

    [SerializeField]
    [Header("현재 해당 슬롯에 위치한 유닛")]
    private UnitData unitData;

    [SerializeField]
    private SpriteRenderer sprite;

    [SerializeField]
    private Animator unitAnimator;

    [SerializeField]
    private Image hpPointBar;

    [SerializeField]
    private Image actionPointBar;

    [SerializeField]
    public GameObject currentTargetIcon;


    [Header("Status")]
    public string unitName;
    public Sprite unitFaceIcon;
    public int maxHp;
    private int _currentHp;
    public int currentHp
    {
        get { return _currentHp; }
        set
        {
            if (_currentHp != value)
            {
                _currentHp = value;
                hpPointBar.fillAmount = currentHp / maxHp;
            }
        }
    }
    public int atk;
    public float maxActionPoint = 100;
    public float _currentActionPoint = 0;
    public float currentActionPoint
    {
        get { return _currentActionPoint; }
        set
        {
            if (_currentActionPoint != value)
            {
                _currentActionPoint = value;
                actionPointBar.fillAmount = currentActionPoint / maxActionPoint;
            }
        }
    }

    public int speed;
    public UnitState unitState;
    public List<SkillData> skillDatas;


    private void Start()
    {
        StatusInit();
    }

    public void StatusInit()
    {
        unitName = unitData.unitName;
        unitFaceIcon = unitData.unitFaceIcon;
        maxHp = unitData.hp;
        currentHp = unitData.hp;
        atk = unitData.atk;
        maxActionPoint = unitData.actionPoint;
        currentActionPoint = 0;
        speed = unitData.speed;
        skillDatas = unitData.skillDatas;
    }

    public void SetAnim(int animNum)
    {
        unitAnimator.SetInteger("State", animNum);
    }

    public void SetUnitState(UnitState changeState)
    {
        unitState = changeState;

        switch (unitState)
        {
            case UnitState.normal:
                SetAnim(0);
                    break;
            case UnitState.attack:
                SetAnim(1);
                break;
            case UnitState.hit:
                SetAnim(2);
                break;
            default:
                break;
        }
    }

    public void Hit(int damage)
    {
        SetUnitState(UnitState.hit);
        currentHp =- damage;
    }
}
