using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSlotController : MonoBehaviour
{
    GameManager dataManager;

    [SerializeField]
    [Header("ÇöÀç À§Ä¡ÇÑ FieldÀÇ UnitFieldController")]
    UnitSlotGroupController unitFieldController;

    [Header("À¯´Ö ½½·ÔÀÇ ¹øÈ£")]
    public bool isNull = false;

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
    public SlotGroundSpriteController slotGroundSpriteController;


    [Header("Status")]
    public string unitName;
    public Sprite unitFaceIcon;
    public SpriteRenderer unitSpriteRenderer;
    public Animator unitAnim;
    public float maxHp;
    private float _currentHp;
    public float atk;
    public float maxActionPoint = 100;
    public float _currentActionPoint = 0;
    public int speed;
    public List<SkillData> skillDatas;

    public float currentHp
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
        dataManager = GameManager.Instance;

        StatusInit();
    }

    public void StatusInit()
    {
        if (unitData.name == "Null")
        {
            isNull = true;
            hpPointBar.gameObject.transform.parent.gameObject.SetActive(false);
            actionPointBar.gameObject.SetActive(false);
        }
        else
        {
            hpPointBar.gameObject.transform.parent.gameObject.SetActive(true);
            actionPointBar.gameObject.SetActive(true);
            unitName = unitData.unitName;
            unitSpriteRenderer.sprite = unitData.unitSprite;
            unitAnim.runtimeAnimatorController = unitData.unitAnimController;
            maxHp = unitData.hp;
            currentHp = unitData.hp;
            atk = unitData.atk;
            maxActionPoint = unitData.actionPoint;
            currentActionPoint = 0;
            speed = unitData.speed;
            skillDatas = unitData.skillDatas;
        }
    }


    public void SetAnim(int animNum)
    {
        unitAnimator.SetInteger("State", animNum);
    }

    public void SetActionPointBar(float setActionPoint)
    {
        actionPointBar.fillAmount = setActionPoint / 100f;
    }

    public void SetDirection(bool isRight)
    {
        sprite.flipX = !isRight;
    }

    public void ChangeUnit(UnitData changeUnitData)
    {
        unitData = changeUnitData;

        SetAnim(0);
        SetActionPointBar(0.0f);

        StatusInit();
    }
}
