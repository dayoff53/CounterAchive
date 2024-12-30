using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class UnitBase : MonoBehaviour
{
    [Header("매니저 참조")]
    [Tooltip("스테이지 매니저 인스턴스")]
    StageManager stageManager;
    [Tooltip("카메라 매니저 인스턴스")]
    CameraManager cameraManager;
    [Tooltip("데이터 매니저 인스턴스")]
    DataManager dataManager;

    [Space(20)]
    [Header("------------------- 시각적 요소 -------------------")]
    [Tooltip("유닛의 스프라이트 렌더러")]
    [SerializeField] public SpriteRenderer spriteRenderer;
    [Tooltip("유닛의 애니메이터")]
    [SerializeField] private Animator unitAnimator;

    [Space(20)]
    [Header("------------------- UI -------------------")]
    [Tooltip("HP 바 이미지")]
    [SerializeField] private Image hpPointBar;
    [Tooltip("HP 텍스트")]
    [SerializeField] private TextMeshProUGUI hpPointText;
    [Tooltip("AP 바 이미지")]
    [SerializeField] private Image actionPointBar;

    [Space(10)]
    [Header("스킬 타겟 관련 UI")]
    [Tooltip("스킬 타겟 아이콘 리스트 (현재 턴 유닛, 타겟 유닛)")]
    [SerializeField] private List<Image> skillTargetingImageList;
    [Tooltip("스킬 타겟 텍스트")]
    [SerializeField] private TextMeshProUGUI skillTargetingText;

    [Space(20)]
    [Header("------------------- SFX -------------------")]
    [Header("게임플레이 연출")]
    [Tooltip("히트 연출 위치 (히트 연출 위치, 유닛의 눈 위치)")]
    /// <summary>
    /// 히트 이펙트 위치 (유닛의 중심 위치, 유닛의 눈 위치)
    /// </summary>
    [SerializeField] public List<GameObject> productionPositionList;
    [Tooltip("시체 디졸브 효과")]
    [SerializeField] public CorpseProduction corpseDissolve;

    [Space(20)] 
    [Header("------------------- UnitData -------------------")]
    [Tooltip("유닛 데이터")]
    public UnitData unitData;
    [Tooltip("유닛 고유 번호")]
    public int unitNumber;
    [Tooltip("유닛 이름")]
    public string unitName;
    [Tooltip("유닛 얼굴 아이콘")]
    public Sprite unitFaceIcon;
    [Tooltip("유닛 애니메이션 컨트롤러")]
    public RuntimeAnimatorController unitAnim;

    [Space(20)]
    [Header("------------------- UnitStatus -------------------")]
    [Tooltip("최대 체력")]
    public float maxHp;
    [Tooltip("현재 체력")]
    private float _currentHp;
    [Tooltip("최대 행동력")]
    public float maxAp = 100;
    [Tooltip("현재 행동력")]
    public float _currentAp = 0;
    [Tooltip("공격력")]
    public float atk;
    [Tooltip("방어력")]
    public float def;
    [Tooltip("명중률")]
    public float acc = 1;
    [Tooltip("회피율")]
    public float eva = 1;
    [Tooltip("속도")]
    public float speed;
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
    
    [Space(10)]
    [Header("특성 및 스킬")]
    [Tooltip("유닛 태그 목록")]
    public List<UnitTag> unitTagList;
    [Tooltip("스킬 데이터 목록")]
    public List<SkillData> skillDataList;



    private void Start()
    {
        stageManager = StageManager.Instance;
        cameraManager = CameraManager.Instance;
        dataManager = DataManager.Instance;

        UnitDataInit(unitData);
    }

    /// <summary>
    /// UnitData를 기반으로 UnitBase의 데이터를 초기화합니다.
    /// </summary>
    /// <param name="setUnitData">초기화할 UnitData 객체</param>
    public void UnitDataInit(UnitData setUnitData)
    {
        if (setUnitData == null)
            unitData = dataManager.unitDataList.Find(n => n.unitNumber == 0);
        else
            unitData = setUnitData;
        

        if (unitData.unitNumber == 0)
        {
            unitNumber = unitData.unitNumber;
            unitName = unitData.name;
            unitAnimator.runtimeAnimatorController = null;
            spriteRenderer.sprite = null;
            unitFaceIcon = null;
            maxHp = unitData.hp;
            currentHp = unitData.hp;
            atk = unitData.atk;
            def = unitData.def;
            maxAp = unitData.ap;
            currentAp = 0;
            speed = unitData.speed;
            skillDataList = unitData.skillDataList;

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
            corpseDissolve.CorpseInit();

            SetAnim(0);
            actionPointBar.fillAmount = 0f / 100f;
        }
        
        SetSkillTargeting(false, "");
        SetTurn(false);
    }
    /// <summary>
    /// UnitStatus를 기반으로 UnitBase의 데이터를 초기화합니다.
    /// </summary>
    /// <param name="setStatus">초기화할 UnitStatus 객체</param>
    public void SetStatus(UnitStatus setStatus)
    {
        if (setStatus.unitData == null)
        {
            UnitDataInit(dataManager.unitDataList.Find(n => n.unitNumber == 0));
        }
        else
        {
            UnitDataInit(setStatus.unitData);
        }
        unitNumber = setStatus.unitNumber;
        spriteRenderer.sprite = dataManager.unitDataList.Find(n => n.unitNumber == unitNumber).unitSprite;
        unitFaceIcon = dataManager.unitDataList.Find(n => n.unitNumber == unitNumber).unitFaceIcon;

        if (!setStatus.isOriginal)
        {
            unitAnim = setStatus.unitData.unitAnimController;
            unitAnimator.runtimeAnimatorController = unitAnim;
            maxHp = setStatus.maxHp;
            currentHp = setStatus.currentHp;
            atk = setStatus.atk;
                def = setStatus.def;
            acc = setStatus.acc;
            eva = setStatus.eva;
            speed = setStatus.speed;
            maxAp = setStatus.ap;
            currentAp = 0;
            skillDataList = new List<SkillData>();
            foreach (int skillNum in setStatus.skillNumberList)
            {
                skillDataList.Add(dataManager.skillList[skillNum]);
            }
        }
    }
    

/// <summary>
/// UnitBase의 데이터를 갱신합니다.
/// </summary>
    public void UnitBaseUpdate()
    {
        if (unitData.unitNumber == 0)
        {
            UnitDataInit(unitData);
        }
        else
        {
            hpPointBar.gameObject.transform.parent.gameObject.SetActive(true);
            actionPointBar.gameObject.SetActive(true);

            SetAnim(0);
            actionPointBar.fillAmount = 0f / 100f;
            SetSkillTargeting(false, "");
            SetTurn(false);
        }
    }


/// <summary>
/// 유닛의 애니메이션을 설정합니다. (0 = 기본, 1 = 공격, 2 = 피격)
/// </summary>
/// <param name="animNum">애니메이션 번호</param>
    public void SetAnim(int animNum)
    {
        unitAnimator.SetInteger("State", animNum);
    }
    public void SetDirection(bool isLeft)
    {
        spriteRenderer.flipX = isLeft;
    }

    public void SetTurn(bool isUseTurn)
    {
        if (isUseTurn)
        {
            skillTargetingImageList[0].gameObject.SetActive(true);
        }
        else
        {
            skillTargetingImageList[0].gameObject.SetActive(false);
        }
    }



    public void Damage(float damage)
    {
        if (unitData.name != "Null")
        {
            currentHp -= ComputeDamage(damage);

        if (currentHp <= 0)
            {
                Death(damage);
            }
        }
    }

    public float ComputeDamage(float damage)
    {
        if (damage / 2 > def)
        {
            damage = damage - def;
        }
        else
        {
            damage = (damage / 2) - ((def - (damage / 2)) / 2);
        }

        damage = (int)damage;

        if (damage <= 1)
        {
            damage = 1;
        }
        return (int)damage;
    }

    public void Death(float pushForce)
    {
        stageManager.isUnitDying = true;
        stageManager.lastEnemyDeathObject = corpseDissolve.gameObject;

        corpseDissolve.DissolveStart(spriteRenderer);
        if (spriteRenderer.flipX)
            corpseDissolve.PushUnit(pushForce, new Vector2(1, 0));
        else
            corpseDissolve.PushUnit(pushForce, new Vector2(-1, 0));
        spriteRenderer.sprite = null;

        // UnitData를 Null로 변경하여 유닛을 비활성화합니다.
        unitData = dataManager.unitDataList.Find(un => un.unitNumber == 0); // Null 값을 갖는 UnitData로 설정
    }
    public void HitProduction(GameObject hitProductonObject, float skillHitRadius)
    {
        Vector3 hitPos = productionPositionList[0].gameObject.transform.position;
        hitProductonObject.transform.position = hitPos;

        if (skillHitRadius > 0)
        {
            float angle = Random.Range(0, 360);

            float randomRadius = Random.Range(0f, skillHitRadius);

            float x = hitPos.x + randomRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = hitPos.y + randomRadius * Mathf.Sin(angle * Mathf.Deg2Rad);

            hitProductonObject.transform.position = new Vector3(x, y, hitPos.z);
        }

        // 피격 시 흔들림 효과 추가
        StartCoroutine(ShakeUnit());
    }
    private IEnumerator ShakeUnit()
    {
        float shakeDuration = 0.2f;
        float shakeAmount = 0.1f;
        
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            float x = unitData.unitPosition.x + Random.Range(-1f, 1f) * shakeAmount;
            float y = unitData.unitPosition.y + Random.Range(-1f, 1f) * shakeAmount;
            
            transform.localPosition = new Vector3(x, y, unitData.unitPosition.z);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.localPosition = unitData.unitPosition;
    }

    public void SetSkillTargeting(bool isTargeting, string targetingText)
    {
        if (isTargeting)
        {
            skillTargetingImageList[1].gameObject.SetActive(true);
            skillTargetingText.text = targetingText;
        }
        else
        {
            skillTargetingImageList[1].gameObject.SetActive(false);
            skillTargetingText.text = "";
        }
    }
}
