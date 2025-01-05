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
    [Tooltip("풀 매니저 인스턴스")]
    PoolManager poolManager;

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
    public List<GameObject> productionPositionList;
    [SerializeField]
    private GameObject damageTextObject;
    [Tooltip("시체 디졸브 효과")]
    public UnitProduction corpseDissolve;
    

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
    public bool isFlipX
    {
        get { return spriteRenderer.flipX; }
        set { spriteRenderer.flipX = value; }
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
        poolManager = PoolManager.Instance;

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
            spriteRenderer.color = new Color(1, 1, 1, 0);
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
            spriteRenderer.color = new Color(1, 1, 1, 1);
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

            spriteRenderer.sortingOrder = (int)stageManager.unitStateColorsObject.orderLayerNumber[0];
            SetAnim(0);
            SetSkillTargeting(false, "");
            
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


    public float ComputeDamage(float damage, float defPierce)
    {
        float computedDamage = damage;
        if (damage / 2 > def)
        {
            computedDamage = damage - def;
        }
        else
        {
            computedDamage = (damage / 2) - ((def - (damage / 2)) / 2);
        }

        computedDamage = (int)computedDamage;

        if (computedDamage <= 1)
        {
            computedDamage = 1;
        }

        //방어 관통 수치 계산
        computedDamage += (damage - computedDamage) * defPierce;
        computedDamage = Mathf.Max(damage);

        Debug.Log($"ComputeDamage : {damage}"); 
        return (int)computedDamage;
    }


    public void Damage(float damage, float defPierce)
    {
        if (unitData.name != "Null")
        {
            float computedDamage = ComputeDamage(damage, defPierce);

            // 데미지 계산
            currentHp -= computedDamage;

            // 데미지 텍스트 생성 및 애니메이션
            GameObject damageTextObj = poolManager.Pop(damageTextObject);
            if (damageTextObj != null)
            {
                damageTextObj.transform.position = productionPositionList[0].transform.position + Vector3.up * 1.5f;
                TextMeshProUGUI damageText = damageTextObj.GetComponent<TextMeshProUGUI>();
                damageText.text = computedDamage.ToString();
                damageText.color = new Color(1, 1, 1, 1);

                // 데미지 텍스트 애니메이션 코루틴 시작
                StartCoroutine(DamageTextAnimation(damageText));
            }

            if (currentHp <= 0)
            {
                Death(damage);
            }
        }
    }

    public void Damage(float damage)
    {
        Damage(damage, 0);
    }

    private IEnumerator DamageTextAnimation(TextMeshProUGUI damageText)
    {
        float duration = 1.0f;
        float elapsedTime = 0f;
        Vector3 startPos = damageText.transform.position;
        Color startColor = damageText.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = 1 - (elapsedTime / duration);
            
            // 위로 올라가는 움직임
            damageText.transform.position = startPos + Vector3.up * (elapsedTime * 2f);
            
            // 투명도 조절
            damageText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            
            yield return null;
        }

        Destroy(damageText.gameObject);
    }


    public void Death(float pushForce)
    {
        stageManager.isUnitDying = true;
        stageManager.lastEnemyDeathObject = corpseDissolve.gameObject;

        corpseDissolve.DissolveStart(spriteRenderer);

        float randomDirectionY = Random.Range(0.35f, 0.85f);

        if(pushForce > 5)
        {
            float excess = pushForce - 5;
            float reduction = 1 - (excess * 0.15f); // 5를 초과하는 값마다 15%씩 효율 감소
            pushForce = 5 + (excess * reduction);
        }
        pushForce = Mathf.Min(pushForce, 10);

        if (stageManager.unitSlotList[stageManager.currentTurnSlotNumber].unit.isFlipX)
        {
            corpseDissolve.PushUnit(pushForce, new Vector2(-1, randomDirectionY));
        }
        else
        {
            corpseDissolve.PushUnit(pushForce, new Vector2(1, randomDirectionY));
        }
        spriteRenderer.color = new Color(1, 1, 1, 0);

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
