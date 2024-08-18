using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using TMPro;

/// <summary>
/// 턴 종류 (대기 => 유닛 플레이 => 스킬 대상 선택 => 스킬 이펙트 재생)
/// </summary>
public enum turnState
{
    Stay,
    UnitPlay,
    SkillSelect,
    SkillPlay
}

public class InGameManager : Singleton<InGameManager>
{
    [Header("UnitSlot")]
    [Tooltip("유닛 슬롯 리스트")]
    public List<UnitSlotController> unitSlots;
    [Tooltip("행동력 누적을 위한 Dictionary")]
    public SerializableDictionary<UnitSlotController, float> actionPoints = new SerializableDictionary<UnitSlotController, float>();
    [SerializeField]
    [Tooltip("유닛이 사용할 코스트")]
    private float _cost;
    [Tooltip("기술 사용을 위한 cost")]
    public float cost 
    {
        get { return _cost; }
        set 
        {
            if(_cost != value)
            {
                _cost = value;
                costBar.fillAmount = _cost / 10;
                costGauge.fillAmount = _cost - Mathf.Floor(_cost);
                if (_cost > 10)
                {
                    costGauge.fillAmount = 1;
                }
                costText.text = Mathf.FloorToInt(_cost).ToString();
            }
        }
    }

    [SerializeField]
    private Image costGauge;
    [SerializeField]
    private TMP_Text costText;
    [SerializeField]
    private Image costBar;

    [SerializeField]
    [Tooltip("현재 턴을 지닌 유닛 식별표")]
    public GameObject currentTurnSlotIcon;

    [SerializeField]
    [Header("SkillSlot")]
    [Tooltip("스킬 슬롯 리스트")]
    public List<SkillSlotUIController> skillSlot;

    [SerializeField]
    [Tooltip("각 슬롯의 원래 위치를 저장할 딕셔너리")]
    private SerializableDictionary<GameObject, Vector3> originalPositions = new SerializableDictionary<GameObject, Vector3>();

    /// <summary>
    /// 현재 턴을 행사 중인 슬롯의 번호
    /// </summary>
    public int currentTurnSlotNumber;

    /// <summary>
    /// 유닛의 이동 중 상태를 판단하는 값
    /// </summary>
    [SerializeField]
    private bool isMoving = false;
 
    [SerializeField]
    private turnState _currentTurn = turnState.Stay;

    public turnState currentTurn
    {
        get 
        { return _currentTurn; }
        set
        {
            switch (_currentTurn)
            {
                default:
                    break;
            }

            _currentTurn = value;
        }
    }

    public List<Color> unitStateColors;
    public SkillSlotUIController currentSkillSlot;

    void Start()
    {
        Init();
    }

    /// <summary>
    /// 게임 초기 설정을 실행합니다.
    /// </summary>
    private void Init()
    {
        SlotPosInit();
        ActionPointsInit();
        StartCoroutine(ActionPointAccumulation());
    }

    /// <summary>
    /// 슬롯 위치 초기화를 담당합니다. (현재는 초기 바라보는 방향만 담당)
    /// </summary>
    public void SetUnitSlot(List<UnitSlotController> setUnitSlots)
    {
        unitSlots = setUnitSlots;

        for (int i = setUnitSlots.Count - 1; setUnitSlots.Count - 6 < i; i--)
        {
            setUnitSlots[i].SetDirection(false);
        }
    }

    #region Turn

    /// <summary>
    /// 각 unitSlot의 actionPoints를 초기화합니다.
    /// </summary>
    private void ActionPointsInit()
    {
        currentTurn = turnState.Stay;
        foreach (var unit in unitSlots)
        {
            if (unit != null && unit != null)
            {
                if (!actionPoints.ContainsKey(unit))
                {
                    actionPoints.Add(unit, unit.currentActionPoint); // 키가 없으면 추가
                }
            }
        }
    }

    /// <summary>
    /// actionPoints를 지속적으로 각 유닛의 속도만큼 증가시킵니다.
    /// </summary>
    private IEnumerator ActionPointAccumulation()
    {
        while (true)
        {
            switch (currentTurn)
            {
                case turnState.Stay:
                foreach (var unit in unitSlots)
                {
                    if (unit != null)
                    {
                        if (actionPoints.TryGetValue(unit, out float currentPoints))
                        {
                            actionPoints[unit] = currentPoints + unit.speed * Time.deltaTime;
                            unit.currentActionPoint = actionPoints[unit];


                            CostCharger(unit);
                        }
                        else
                        {
                            Debug.LogWarning($"No key found for {unit.name}. Adding key.");
                            actionPoints.Add(unit, unit.speed * Time.deltaTime);  // 키가 없을 경우 추가
                            unit.currentActionPoint = actionPoints[unit];
                        }
                    }
                }
                GetNextTurnSlotNumber();
                    break;

                case turnState.SkillSelect:

                    break;

                default:
                    break;
            }
            yield return null;
        }
    }

    private void CostCharger(UnitSlotController unit)
    {
            if (unit != null)
            {
                if (unit.unitTeam == 0)
                {
                    float currentSpeed = unit.speed;

                    if (cost < 10)
                    {
                        cost += (currentSpeed / 20) * Time.deltaTime;
                    }
                    else if (cost >= 10)
                    {
                        cost = 10;
                    }
                }
            }
        
    }

    /// <summary>
    /// 다음 턴을 실행할 유닛을 결정합니다.
    /// </summary>
    private void GetNextTurnSlotNumber()
    {
        for (int i = 0; i < unitSlots.Count; i++)
        {
            if (actionPoints[unitSlots[i]] >= unitSlots[i].maxActionPoint)
            {
                currentTurnSlotNumber = i;

                UnitSlotController currentTurnSlot = unitSlots[currentTurnSlotNumber];
                ExecuteTurn(currentTurnSlot);
                break;
            }
        }
    }

    /// <summary>
    /// 유닛의 턴을 실행합니다.
    /// </summary>
    private void ExecuteTurn(UnitSlotController unit)
    {
        currentTurn = turnState.UnitPlay;

        //초기화
        currentTurnSlotNumber = unitSlots.IndexOf(unit);
        SkillSlotInit(unitSlots[currentTurnSlotNumber].skillDatas);
        UnitSlotController currentTurnSlot = unitSlots[currentTurnSlotNumber];

        //현재 턴을 가진 유닛 구분
        unit.currentTargetIcon.SetActive(true);
        SlotGroundSpriteController groundSprite = unitSlots[currentTurnSlotNumber].slotGroundSpriteController;
        groundSprite.SetSlotGroundState(SlotGroundState.Select, unitStateColors[1]);
        currentTurnSlot.currentTargetIcon.SetActive(true);

        //액션 포인트 초기화
        actionPoints[currentTurnSlot] = 0;
        Debug.Log("턴을 시작합니다. 현재 턴은 " + unitSlots[0].name + " (" + unitSlots[0].speed + " 속도) 유닛입니다.");
    }

    /// <summary>
    /// 턴 종료 후 일정 시간 대기를 관리합니다.
    /// </summary>
    public IEnumerator DelayTurnEnd(float delay)
    {
        yield return new WaitForSeconds(delay); // 지정된 시간만큼 대기
        TurnEnd(); // 대기 후 호출할 함수
    }

    /// <summary>
    /// 턴을 종료합니다.
    /// </summary>
    public void TurnEnd()
    {
        unitSlots[currentTurnSlotNumber].SetAnim(0);
        unitSlots[currentTurnSlotNumber].currentTargetIcon.SetActive(false);

        //UnitGround의 색상 및 스프라이트 초기화
        foreach (var unit in unitSlots)
        {
            SlotGroundSpriteController groundSprite = unit.slotGroundSpriteController;
            groundSprite.SetSlotGroundState(SlotGroundState.Normal, unitStateColors[0]);
        }

        currentTurn = turnState.Stay;
    }
    #endregion

    #region Slot
    private void SlotPosInit()
    {
        for (int i = 0; i < unitSlots.Count; i++)
        {
            if (unitSlots[i] != null)
            {
                var unit = unitSlots[i].gameObject;
                if (unit != null)
                {
                    // 초기 유닛 위치 설정
                    originalPositions[unit] = unit.transform.position;
                }
            }
        }
    }

    /// <summary>
    /// 지정된 유닛을 다른 유닛의 위치로 이동시킵니다.
    /// </summary>
    /// <param name="moveUnitNum">이동할 유닛의 인덱스입니다.</param>
    /// <param name="targetUnitNum">목표 유닛의 인덱스입니다.</param>
    public void MoveUnit(int moveUnitNum, int targetUnitNum)
    {
        if (isMoving || unitSlots[moveUnitNum] == null || unitSlots[targetUnitNum] == null)
        {
            Debug.LogError("Movement is currently locked or invalid slot numbers provided.");
            return;
        }

        isMoving = true;

        GameObject moveUnit = unitSlots[moveUnitNum].gameObject;
        GameObject targetUnit = unitSlots[targetUnitNum].gameObject;

        moveUnit.transform.DOKill();
        targetUnit.transform.DOKill();

        moveUnit.transform.position = originalPositions[moveUnit];
        targetUnit.transform.position = originalPositions[targetUnit];

        Sequence sequence = DOTween.Sequence();
        sequence.Append(moveUnit.transform.DOMove(originalPositions[targetUnit], 1f).SetEase(Ease.InOutQuad));
        sequence.Join(targetUnit.transform.DOMove(originalPositions[moveUnit], 1f).SetEase(Ease.InOutQuad));
        sequence.OnComplete(() => {
            originalPositions[moveUnit] = moveUnit.transform.position;
            originalPositions[targetUnit] = targetUnit.transform.position;
            UnitSlotController targetSlot = unitSlots[targetUnitNum];
            unitSlots[targetUnitNum] = unitSlots[moveUnitNum];
            unitSlots[moveUnitNum] = targetSlot;
            Debug.Log("Units have been swapped successfully.");
            isMoving = false; // 이동 완료 후 이동 중 상태 해제
        });
    }

    /// <summary>
    /// 현재 턴을 가진 유닛을 오른쪽 또는 왼쪽으로 이동시킵니다.
    /// </summary>
    /// <param name="rightMove">오른쪽으로 이동할지 여부입니다. false면 왼쪽으로 이동합니다.</param>
    public void DirectMoveUnit(bool rightMove)
    {
        int moveTargetUnitNum = currentTurnSlotNumber;

        if (isMoving || unitSlots[currentTurnSlotNumber] == null || unitSlots[moveTargetUnitNum] == null)
        {
            Debug.LogWarning("Movement is currently locked or invalid slot numbers provided.");
            return;
        }

            if (rightMove)
            {
                if (currentTurnSlotNumber < unitSlots.Count)
                {
                    MoveUnit(currentTurnSlotNumber, currentTurnSlotNumber + 1);

                    currentTurnSlotNumber = currentTurnSlotNumber + 1;
                }
            }
            else
            {
                if (currentTurnSlotNumber > 0)
                {
                    MoveUnit(currentTurnSlotNumber, currentTurnSlotNumber - 1);

                    currentTurnSlotNumber = currentTurnSlotNumber - 1;
                }
            }
    }

    #endregion

    #region Skill

    /// <summary>
    /// 스킬 슬롯을 초기화하고 주어진 스킬 데이터로 설정합니다.
    /// </summary>
    /// <param name="setSkillDatas">설정할 스킬 데이터 리스트입니다.</param>
    public void SkillSlotInit(List<SkillData> setSkillDatas)
    {
        for (int i = 0; i < setSkillDatas.Count; i++)
        {
            skillSlot[i].SetSkillData(setSkillDatas[i]);
        }
    }

    /// <summary>
    /// 지정된 범위와 데미지로 공격을 수행합니다.
    /// </summary>
    /// <param name="damage">적용할 데미지입니다.</param>
    public void ExecuteAttack(int targetNum, int damage)
    {
        unitSlots[targetNum].currentHp -= damage;
    }

    #endregion
}
