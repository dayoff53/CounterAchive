using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSlotController : MonoBehaviour
{
    [SerializeField]
    [Header("현재 위치한 Field의 UnitFieldController")]
    UnitFieldMoveController unitFieldController;

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
    public int atk;
    public float maxActionPoint = 100;
    public float _currentActionPoint = 0;
    public int speed;
    public List<SkillData> skillDatas;

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

    public void SetActionPointBar(float setActionPoint)
    {
        actionPointBar.fillAmount = setActionPoint / 100f;
    }

    public void AddActionPoint(float addPoint)
    {
        currentActionPoint += addPoint;
        actionPointBar.fillAmount = currentActionPoint / maxActionPoint;
    }
}
