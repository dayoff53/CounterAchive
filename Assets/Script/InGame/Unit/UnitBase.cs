using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitBase : MonoBehaviour
{
    DataManager dataManager;

    [Header("유닛의 팀")]
    public int unitTeam = 0;

    [Header("연동될 컴포넌트")]
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private Animator unitAnimator;

    [SerializeField]
    private Image hpPointBar;

    [SerializeField]
    private Image actionPointBar;


    [Header("Status")]
    public UnitData unitData;
    public string unitName;
    public Sprite unitFaceIcon;
    public RuntimeAnimatorController unitAnim;
    public float maxHp;
    private float _currentHp;
    public float atk;
    public float maxActionPoint = 100;
    public float _currentActionPoint = 0;
    public int speed;
    public List<SkillData> skillDataList;


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
        dataManager = DataManager.Instance;

        StatusInit();
    }

    public void StatusInit()
    {
        if (unitData.name == "Null")
        {
            hpPointBar.gameObject.transform.parent.gameObject.SetActive(false);
            actionPointBar.gameObject.SetActive(false);
        }
        else
        {
            hpPointBar.gameObject.transform.parent.gameObject.SetActive(true);
            actionPointBar.gameObject.SetActive(true);
            unitName = unitData.name;
            spriteRenderer.sprite = unitData.unitSprite;
            unitFaceIcon = unitData.unitFaceIcon;
            unitAnimator.runtimeAnimatorController = unitData.unitAnimController;
            maxHp = unitData.hp;
            currentHp = unitData.hp;
            atk = unitData.atk;
            maxActionPoint = unitData.actionPoint;
            currentActionPoint = 0;
            speed = unitData.speed;
            skillDataList = unitData.skillDataList;
            
        }
    }

    /// <summary>
    /// 받은 UnitState값에 알맞게 UnitController의 State값을 변경
    /// </summary>
    /// <param name="setState"></param>
    public void SetState(UnitState setState)
    {
        unitData = dataManager.unitDataList.Find(un => un.unitNumber == setState.unitNumber);

            if (!string.IsNullOrEmpty(setState.unitName) && setState.unitName.StartsWith("*"))
            {
                unitName = unitName + setState.unitName.Substring(1);
            } else
        {
            unitName = setState.unitName;
        }

            spriteRenderer.sprite = setState.defaultUnitData.unitSprite;
            unitFaceIcon = setState.defaultUnitData.unitFaceIcon;

        switch(unitName)
        {
            case "Null":
                unitAnim = setState.defaultUnitData.unitAnimController;
                unitAnimator.runtimeAnimatorController = unitAnim;
                maxHp = setState.hp;
                currentHp = setState.hp;
                atk = setState.atk;
                maxActionPoint = setState.actionPoint;
                currentActionPoint = 0;
                speed = setState.speed;
                skillDataList = new List<SkillData>();
                foreach (int skillNum in setState.skillNumberList)
                {
                    skillDataList.Add(dataManager.skillList[skillNum]);
                }
                break;
            case "Default":
                unitAnim = setState.defaultUnitData.unitAnimController;
                unitAnimator.runtimeAnimatorController = unitAnim;
                maxHp = setState.defaultUnitData.hp;
                currentHp = setState.defaultUnitData.hp;
                atk = setState.defaultUnitData.atk;
                maxActionPoint = setState.defaultUnitData.actionPoint;
                currentActionPoint = 0;
                speed = setState.defaultUnitData.speed;
                skillDataList = new List<SkillData>();
                foreach (int skillNum in setState.skillNumberList)
                {
                    skillDataList.Add(dataManager.skillList[skillNum]);
                }
                break;
            default:
                break;
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
        spriteRenderer.flipX = !isRight;
    }

}
