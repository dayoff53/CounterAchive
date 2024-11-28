using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// ���� ������ ������
/// </summary>
[System.Serializable]
public enum PublicUnitStatus
{
    maxHp,
    currentHp,
    atk,
    def,
    maxActionPoint,
    currentActionPoint,
    speed,
}

public class UnitBase : MonoBehaviour
{
    DataManager dataManager;

    [Header("������ ��")]
    public int unitTeam = 0;

    [Header("������ ������Ʈ")]
    [SerializeField]
    public SpriteRenderer spriteRenderer;

    [SerializeField]
    private Animator unitAnimator;

    [SerializeField]
    private Image hpPointBar;
    [SerializeField]
    private TextMeshProUGUI hpPointText;

    [SerializeField]
    private Image actionPointBar;

    [SerializeField]
    public GameObject hitPosition;


    [Header("Static Status")]
    public UnitData unitData;
    public string unitName;
    public Sprite unitFaceIcon;
    public RuntimeAnimatorController unitAnim;
    public List<SkillData> skillDataList;


    [Header("Public Status")]
    public float maxHp;
    private float _currentHp;
    public float atk;
    public float def;
    public float maxAP = 100;
    public float _currentActionPoint = 0;
    public float speed;
    public float currentHp
    {
        get { return _currentHp; }
        set
        {
            if (_currentHp != value)
            {
                _currentHp = value;

                hpPointBar.fillAmount = ((float)currentHp / (float)maxHp);


                hpPointText.text = $"{currentHp}/{maxHp}";
            }
        }
    }
    public float currentAP
    {
        get { return _currentActionPoint; }
        set
        {
            if (_currentActionPoint != value)
            {
                _currentActionPoint = value;

                actionPointBar.fillAmount = currentAP / maxAP;
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
            hpPointText.text = "";
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
            def = unitData.def;
            maxAP = unitData.actionPoint;
            currentAP = 0;
            speed = unitData.speed;
            skillDataList = unitData.skillDataList;
            
        }
    }

    /// <summary>
    /// ���ϴ� �����̴����� ���� �����մϴ�.
    /// </summary>
    /// <param name="statusToUpdate"></param>
    /// <param name="newValue"></param>
    public void SetStatus(PublicUnitStatus statusToUpdate, float newValue)
    {
        switch (statusToUpdate)
        {
            case PublicUnitStatus.maxHp:
                maxHp = newValue;
                // ���� HP�� �ִ� HP�� �ʰ����� �ʵ��� ����
                currentHp = Mathf.Clamp(currentHp, 0, maxHp);
                break;

            case PublicUnitStatus.currentHp:
                currentHp = newValue;
                break;

            case PublicUnitStatus.atk:
                atk = newValue;
                break;

            case PublicUnitStatus.def:
                def = newValue;
                break;

            case PublicUnitStatus.maxActionPoint:
                maxAP = newValue;
                // ���� �׼� ����Ʈ�� �ִ�ġ�� �ʰ����� �ʵ��� ����
                currentAP = Mathf.Clamp(currentAP, 0, maxAP);
                break;

            case PublicUnitStatus.currentActionPoint:
                currentAP = newValue;
                break;

            case PublicUnitStatus.speed:
                speed = newValue;
                break;

            default:
                Debug.LogWarning("Unknown status type");
                break;
        }
    }

    /// <summary>
    /// ���� UnitState���� �˸°� UnitController�� Status���� ����
    /// </summary>
    /// <param name="setStatus">�ʱ�ȭ �� �������ͽ�</param>
    public void SetStatus(UnitStatus setStatus)
    {
        unitData = dataManager.unitDataList.Find(un => un.unitNumber == setStatus.unitNumber);

        if (!string.IsNullOrEmpty(setStatus.unitName) && setStatus.unitName.StartsWith("*"))
        {
            unitName = unitName + setStatus.unitName.Substring(1);
        }
        else
        {
            unitName = setStatus.unitName;
        }

        spriteRenderer.sprite = setStatus.defaultUnitData.unitSprite;
        unitFaceIcon = setStatus.defaultUnitData.unitFaceIcon;

        switch (unitName)
        {
            case "Null":
                unitAnim = setStatus.defaultUnitData.unitAnimController;
                unitAnimator.runtimeAnimatorController = unitAnim;
                maxHp = setStatus.hp;
                currentHp = setStatus.hp;
                atk = setStatus.atk;
                maxAP = setStatus.actionPoint;
                currentAP = 0;
                speed = setStatus.speed;
                skillDataList = new List<SkillData>();
                foreach (int skillNum in setStatus.skillNumberList)
                {
                    skillDataList.Add(dataManager.skillList[skillNum]);
                }
                break;
            case "Default":
                unitAnim = setStatus.defaultUnitData.unitAnimController;
                unitAnimator.runtimeAnimatorController = unitAnim;
                maxHp = setStatus.defaultUnitData.hp;
                currentHp = setStatus.defaultUnitData.hp;
                atk = setStatus.defaultUnitData.atk;
                maxAP = setStatus.defaultUnitData.actionPoint;
                currentAP = 0;
                speed = setStatus.defaultUnitData.speed;
                skillDataList = new List<SkillData>();
                foreach (int skillNum in setStatus.skillNumberList)
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

    public void Damage(float damage)
    {
        if (damage / 2 > def)
        {
            damage = damage - def;
        } else
        {
            damage = (damage / 2) - ((def - (damage / 2)) / 2);
        }

        if(damage <= 1)
        {
            damage = 1;
        }

        SetStatus(PublicUnitStatus.currentHp, damage);
    }
}
