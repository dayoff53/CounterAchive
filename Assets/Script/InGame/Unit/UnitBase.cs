using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;



public class UnitBase : MonoBehaviour
{
    PoolManager poolManager;
    DataManager dataManager;

    [Header("유닛의 팀")]
    public int unitTeam = 0;

    [Header("연동될 컴포넌트")]
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

    [SerializeField]
    public GameObject corpseObject;


    [Header("Static Status")]
    public UnitData unitData;
    public int unitNumber;
    public string unitName;
    public Sprite unitFaceIcon;
    public RuntimeAnimatorController unitAnim;
    private bool isDeath = false;


    [Header("Public Status")]
    public float maxHp;
    private float _currentHp;
    public float maxAp = 100;
    public float _currentAp = 0;
    public float atk;
    public float def;
    public float acc = 1;
    public float eva = 1;
    public float speed;
    public List<UnitTag> unitTagList;
    public List<SkillData> skillDataList;
    public float currentHp
    {
        get { return _currentHp; }
        set
        {
            _currentHp = Mathf.Clamp(value, 0, maxHp);

            hpPointBar.fillAmount = ((float)currentHp / (float)maxHp);
            hpPointText.text = $"{currentHp}/{maxHp}";
        }
    }
    public float currentAp
    {
        get { return _currentAp; }
        set
        {
                _currentAp = value;

                actionPointBar.fillAmount = currentAp / maxAp;
        }
    }


    private void Start()
    {
        poolManager = PoolManager.Instance;
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
            unitNumber = unitData.unitNumber;
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
            maxAp = unitData.ap;
            currentAp = 0;
            speed = unitData.speed;
            skillDataList = unitData.skillDataList;
        }
    }

    /// <summary>
    /// 원하는 스테이더스의 값을 변경합니다.
    /// </summary>
    /// <param name="statusToUpdate"></param>
    /// <param name="newValue"></param>
    public void SetStatus(PublicUnitStatusState statusToUpdate, float newValue)
    {
        switch (statusToUpdate)
        {
            case PublicUnitStatusState.maxHp:
                maxHp = newValue;
                // 현재 HP가 최대 HP를 초과하지 않도록 조정
                currentHp = Mathf.Clamp(currentHp, 0, maxHp);
                break;

            case PublicUnitStatusState.currentHp:
                currentHp = newValue;
                break;

            case PublicUnitStatusState.atk:
                atk = newValue;
                break;

            case PublicUnitStatusState.def:
                def = newValue;
                break;

            case PublicUnitStatusState.maxAp:
                maxAp = newValue;
                // 현재 액션 포인트가 최대치를 초과하지 않도록 조정
                currentAp = Mathf.Clamp(currentAp, 0, maxAp);
                break;

            case PublicUnitStatusState.currentActionPoint:
                currentAp = newValue;
                break;

            case PublicUnitStatusState.speed:
                speed = newValue;
                break;

            default:
                Debug.LogWarning("Unknown status type");
                break;
        }
    }

    /// <summary>
    /// 받은 UnitState값에 알맞게 UnitBase의 Status값을 변경
    /// </summary>
    /// <param name="setStatus">초기화 할 스테이터스</param>
    public void SetStatus(UnitStatus setStatus)
    {
        if(setStatus.defaultUnitData != null)
        {
            unitData = setStatus.defaultUnitData;
            unitNumber = unitData.unitNumber;
        }
        else
        {
            unitData = dataManager.unitDataList.Find(un => un.unitNumber == setStatus.unitNumber);
            unitNumber = unitData.unitNumber;
        }


        if (!string.IsNullOrEmpty(setStatus.unitName) && setStatus.unitName.StartsWith("*"))
        {
            unitName = unitName + setStatus.unitName.Substring(1);
        }
        else
        {
            unitName = setStatus.unitName;
        }

        unitNumber = setStatus.unitNumber;
        spriteRenderer.sprite = setStatus.defaultUnitData.unitSprite;
        unitFaceIcon = setStatus.defaultUnitData.unitFaceIcon;

        switch (unitName)
        {
            case "Null":
                unitAnim = setStatus.defaultUnitData.unitAnimController;
                unitAnimator.runtimeAnimatorController = unitAnim;
                maxHp = setStatus.maxHp;
                currentHp = setStatus.maxHp;
                atk = setStatus.atk;
                maxAp = setStatus.ap;
                currentAp = 0;
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
                maxAp = setStatus.defaultUnitData.ap;
                currentAp = 0;
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

    public void SetDirection(bool isLeft)
    {
        spriteRenderer.flipX = isLeft;
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

        currentHp -= damage;
    }

    public void Death()
    {
        //corpseObject.SetActive(true);

        // UnitData를 Null로 설정하여 죽음을 처리
        unitData = dataManager.unitDataList.Find(un => un.unitNumber == 0); // Null 유닛 데이터로 설정
    }


    private void DeathProduction()
    {
        Debug.Log("DeathProduction");

    }


    public void HitProduction(GameObject hitProductonObject, float skillHitRadius)
    {
        Vector3 hitPos = hitPosition.gameObject.transform.position;
        hitProductonObject.transform.position = hitPos;

        if (skillHitRadius > 0)
        {
            float angle = Random.Range(0, 360);

            float randomRadius = Random.Range(0f, skillHitRadius);

            float x = hitPos.x + randomRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = hitPos.y + randomRadius * Mathf.Sin(angle * Mathf.Deg2Rad);

            hitProductonObject.transform.position = new Vector3(x, y, hitPos.z);
        }
    }
}
