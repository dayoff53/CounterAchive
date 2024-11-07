using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSlotController : MonoBehaviour
{
    StageManager gameManager;
    DataManager dataManager;

    /// <summary>
    /// ���� ��ġ�� Field�� UnitFieldController
    /// </summary>
    [SerializeField]
    [Header("���� ��ġ�� Field�� UnitFieldController")]
    UnitSlotGroupController unitFieldController;

    /// <summary>
    /// ���� ������ ����ִ��� ����
    /// </summary>
    [Header("���� ������ ����ִ��� ����")]
    public bool isNull = false;

    /// <summary>
    /// ������ ��
    /// </summary>
    [Header("������ ��")]
    public int unitTeam = 0;

    /// <summary>
    /// ���� ������ ��ȣ
    /// </summary>
    [Header("���� ������ ��ȣ")]
    public int slotNum = 0;

    /// <summary>
    /// ���� �ش� ���Կ� ��ġ�� ����
    /// </summary>
    [SerializeField]
    [Header("���� �ش� ���Կ� ��ġ�� ����")]
    private UnitState unitState;

    [SerializeField]
    private SpriteRenderer sprite;

    [SerializeField]
    private Animator unitAnimator;

    [SerializeField]
    private Image hpPointBar;

    [SerializeField]
    private Image actionPointBar;

    [SerializeField]
    public SlotGroundSpriteController slotGround;


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
    public List<int> skillNumberList;

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
        gameManager = StageManager.Instance;
        dataManager = DataManager.Instance;
    }

    public void StatusInit()
    {
        if (unitState.name == "Null")
        {
            isNull = true;
            hpPointBar.gameObject.transform.parent.gameObject.SetActive(false);
            actionPointBar.gameObject.SetActive(false);
        }
        else if(unitState.name == "SetUnitData")
        {
            isNull = false;
            hpPointBar.gameObject.transform.parent.gameObject.SetActive(true);
            actionPointBar.gameObject.SetActive(true);
            unitName = unitState.defaultUnitData.name;
            unitSpriteRenderer.sprite = unitState.defaultUnitData.unitSprite;
            unitFaceIcon = unitState.defaultUnitData.unitFaceIcon;
            unitAnim.runtimeAnimatorController = unitState.defaultUnitData.unitAnimController;
            maxHp = unitState.defaultUnitData.hp;
            currentHp = unitState.defaultUnitData.hp;
            atk = unitState.defaultUnitData.atk;
            maxActionPoint = unitState.defaultUnitData.actionPoint;
            currentActionPoint = 0;
            speed = unitState.defaultUnitData.speed;
            skillNumberList = new List<int>();
            foreach(SkillData skillData in unitState.defaultUnitData.skillDataList)
            {
                skillNumberList.Add(skillData.skillNumber);
            }
        }
        else
        {
            isNull = false;
            hpPointBar.gameObject.transform.parent.gameObject.SetActive(true);
            actionPointBar.gameObject.SetActive(true);
            unitName = unitState.name;
            unitSpriteRenderer.sprite = dataManager.unitDataList.Find(unit => unit.unitNumber == unitState.unitNumber).unitSprite;
            unitFaceIcon = dataManager.unitDataList.Find(unit => unit.unitNumber == unitState.unitNumber).unitFaceIcon;
            unitAnim.runtimeAnimatorController = dataManager.unitDataList.Find(unit => unit.unitNumber == unitState.unitNumber).unitAnimController;
            maxHp = unitState.hp;
            currentHp = unitState.hp;
            atk = unitState.atk;
            maxActionPoint = unitState.actionPoint;
            currentActionPoint = 0;
            speed = unitState.speed;
            skillNumberList = unitState.skillNumberList;
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

    public void ChangeUnit(UnitState changeUnitState)
    {
        unitState = changeUnitState;

        SetAnim(0);
        SetActionPointBar(0.0f);

        StatusInit();
    }

    public void ChangeUnit(UnitState changeUnitState, int teamNum)
    {
        unitState = changeUnitState;

        unitTeam = teamNum;

        SetAnim(0);
        SetActionPointBar(0.0f);

        StatusInit();
    }
}
