using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSlotController : MonoBehaviour
{
    [SerializeField]
    [Header("ÇöÀç À§Ä¡ÇÑ FieldÀÇ UnitFieldController")]
    UnitSlotGroupController unitFieldController;

    [Header("À¯´ÖÀÇ ÆÀ")]
    public int unitTeam = 0;

    [Header("À¯´Ö ½½·ÔÀÇ ¹øÈ£")]
    public int slotNum = 0;

    [SerializeField]
    [Header("ÇöÀç ÇØ´ç ½½·Ô¿¡ À§Ä¡ÇÑ À¯´Ö")]
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

    [SerializeField]
    public SlotGroundSpriteController slotGroundSpriteController;


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

                hpPointBar.fillAmount = ((float)currentHp / (float)maxHp);
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

    public void SetDirection(bool isRight)
    {
        sprite.flipX = !isRight;
    }
}
