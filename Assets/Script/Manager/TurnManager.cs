using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using TMPro;


public class TurnManager : Singleton<TurnManager>
{
    public TurnState turnState;

    [Tooltip("슬롯 메니저")]
    private UnitManager slotManager;

    //private TurnState turnState = TurnState.FreeTurn;
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
    public UnitSlotController currentTurnSlot;


    // 현재 턴을 행사 중인 슬롯의 번호
    public int currentTurnSlotNumber;


    private void Start()
    {
        slotManager = UnitManager.Instance;
        Init();
    }

    private void Update()
    {
        turnState.Update();
    }


    /// <summary>
    /// 게임 초기 설정을 실행합니다.
    /// </summary>
    private void Init()
    {
        turnState = new TurnState();
        turnState.SetState(new FreeTurnState());
        ActionPointsInit();
    }

    /// <summary>
    /// 슬롯 위치 초기화를 담당합니다.
    /// </summary>
    public void SetUnitSlot(bool isRight, List<UnitSlotController> setUnitSlots)
    {
        if (isRight)
        {
            for (int i = 0; setUnitSlots.Count > i; i++)
            {
                setUnitSlots[i] = slotManager.unitSlots[i + slotManager.middleNum];
            }
        }
        else
        {
            for (int i = 0; setUnitSlots.Count > i; i++)
            {
                setUnitSlots[i] = slotManager.unitSlots[i];
            }
        }
    }

    #region Turn

    /// <summary>
    /// 각 unitSlot의 actionPoints를 초기화합니다.
    /// </summary>
    private void ActionPointsInit()
    {
        foreach (var unit in slotManager.unitSlots)
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


    public void ActionTurn()
    {
        turnState.SetState(new ActionTurnState());
    }

    /// <summary>
    /// 턴을 종료합니다.
    /// </summary>
    public void TurnEnd()
    {
        currentTurnSlot.SetAnim(0);
        currentTurnSlot.currentTargetIcon.SetActive(false);
        turnState.SetState(new FreeTurnState());
    }
    #endregion
}

public interface IState
{
    void Enter();
    void Update();
    void Exit();
}

public class TurnState 
{
    private IState currentState;

    public void SetState(IState newState)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;

        if (currentState != null)
            currentState.Enter();
    }

    public void Update()
    {
        if (currentState != null)
            currentState.Update();
    }
}

public class FreeTurnState : IState
{
    UnitManager slotManager;
    TurnManager turnManager;

    public void Enter()
    {
        slotManager = UnitManager.Instance;
        turnManager = TurnManager.Instance;

        slotManager.UnitInit();
        Debug.Log("FreeTurn: Cost 및 각 Unit의 ActionPoint가 증가합니다.");
    }

    /// <summary>
    /// actionPoints를 지속적으로 각 유닛의 속도만큼 증가시킵니다.
    /// </summary>
    public void Update()
    {
        foreach (var unit in slotManager.unitSlots)
        {
            if (unit != null)
            {
                if (turnManager.actionPoints.TryGetValue(unit, out float currentPoints))
                {
                    turnManager.actionPoints[unit] = currentPoints + unit.speed * Time.deltaTime;
                    unit.currentActionPoint = turnManager.actionPoints[unit];


                    if (unit.slotNum >= 0 && unit.slotNum < 4)
                    {
                        float currentSpeed = unit.speed;
                        if (turnManager.cost <= 10)
                        {
                            turnManager.cost += (currentSpeed / 20) * Time.deltaTime;
                        }
                        else if (turnManager.cost > 10)
                        {
                            turnManager.cost = 10;
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"No key found for {unit.name}. Adding key.");
                    turnManager.actionPoints.Add(unit, unit.speed * Time.deltaTime);  // 키가 없을 경우 추가
                    unit.currentActionPoint = turnManager.actionPoints[unit];
                }
            }
        }

        for (int i = 0; i < slotManager.unitSlots.Count; i++)
        {
            if (turnManager.actionPoints[slotManager.unitSlots[i]] >= slotManager.unitSlots[i].maxActionPoint)
            {
                turnManager.currentTurnSlotNumber = i;

                turnManager.turnState.SetState(new UnitTurnState());
                break;
            }
        }
    }

    public void Exit()
    {
        Debug.Log("UnitTurn 종료.");
    }
}

public class UnitTurnState : IState
{
    UnitManager slotManager;
    TurnManager turnManager;
    SkillManager skillManager;

    public void Enter()
    {
        slotManager = UnitManager.Instance;
        turnManager = TurnManager.Instance;
        skillManager = SkillManager.Instance;

        UnitSlotController unit = slotManager.unitSlots[turnManager.currentTurnSlotNumber];

        unit.currentTargetIcon.SetActive(true);
        slotManager.currentTurnSlotNumber = slotManager.unitSlots.IndexOf(unit);
        turnManager.currentTurnSlot = slotManager.unitSlots[slotManager.currentTurnSlotNumber];

        turnManager.currentTurnSlot.currentTargetIcon.SetActive(true);
        skillManager.SkillSlotsInit(slotManager.unitSlots[slotManager.currentTurnSlotNumber].skillDatas);
        turnManager.actionPoints[turnManager.currentTurnSlot] = 0;
        Debug.Log("턴을 시작합니다. 현재 턴은 " + turnManager.currentTurnSlot.name + " (" + turnManager.currentTurnSlot.speed + " 속도) 유닛입니다.");
    }

    public void Update()
    {
        // 행동 포인트 확인 로직
    }

    public void Exit()
    {
        Debug.Log("UnitTurn 종료.");
    }
}

public class ActionTurnState : IState
{
    public void Enter()
    {
        Debug.Log("ActionTurn: 유닛이 행동을 시작합니다.");
    }

    public void Update()
    {
        // 애니메이션 및 이펙트 출력 로직
    }

    public void Exit()
    {
        Debug.Log("ActionTurn 종료.");
    }
}