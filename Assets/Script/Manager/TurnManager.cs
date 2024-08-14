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

    [Tooltip("���� �޴���")]
    private UnitManager slotManager;

    //private TurnState turnState = TurnState.FreeTurn;
    [Tooltip("�ൿ�� ������ ���� Dictionary")]
    public SerializableDictionary<UnitSlotController, float> actionPoints = new SerializableDictionary<UnitSlotController, float>();
    [SerializeField]
    [Tooltip("������ ����� �ڽ�Ʈ")]
    private float _cost;
    [Tooltip("��� ����� ���� cost")]
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
    [Tooltip("���� ���� ���� ���� �ĺ�ǥ")]
    public UnitSlotController currentTurnSlot;


    // ���� ���� ��� ���� ������ ��ȣ
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
    /// ���� �ʱ� ������ �����մϴ�.
    /// </summary>
    private void Init()
    {
        turnState = new TurnState();
        turnState.SetState(new FreeTurnState());
        ActionPointsInit();
    }

    /// <summary>
    /// ���� ��ġ �ʱ�ȭ�� ����մϴ�.
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
    /// �� unitSlot�� actionPoints�� �ʱ�ȭ�մϴ�.
    /// </summary>
    private void ActionPointsInit()
    {
        foreach (var unit in slotManager.unitSlots)
        {
            if (unit != null && unit != null)
            {
                if (!actionPoints.ContainsKey(unit))
                {
                    actionPoints.Add(unit, unit.currentActionPoint); // Ű�� ������ �߰�
                }
            }
        }
    }


    public void ActionTurn()
    {
        turnState.SetState(new ActionTurnState());
    }

    /// <summary>
    /// ���� �����մϴ�.
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
        Debug.Log("FreeTurn: Cost �� �� Unit�� ActionPoint�� �����մϴ�.");
    }

    /// <summary>
    /// actionPoints�� ���������� �� ������ �ӵ���ŭ ������ŵ�ϴ�.
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
                    turnManager.actionPoints.Add(unit, unit.speed * Time.deltaTime);  // Ű�� ���� ��� �߰�
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
        Debug.Log("UnitTurn ����.");
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
        Debug.Log("���� �����մϴ�. ���� ���� " + turnManager.currentTurnSlot.name + " (" + turnManager.currentTurnSlot.speed + " �ӵ�) �����Դϴ�.");
    }

    public void Update()
    {
        // �ൿ ����Ʈ Ȯ�� ����
    }

    public void Exit()
    {
        Debug.Log("UnitTurn ����.");
    }
}

public class ActionTurnState : IState
{
    public void Enter()
    {
        Debug.Log("ActionTurn: ������ �ൿ�� �����մϴ�.");
    }

    public void Update()
    {
        // �ִϸ��̼� �� ����Ʈ ��� ����
    }

    public void Exit()
    {
        Debug.Log("ActionTurn ����.");
    }
}