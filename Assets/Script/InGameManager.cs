using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using TMPro;

public class InGameManager : Singleton<InGameManager>
{
    [Header("UnitSlot")]
    [Tooltip("���� ���� ����Ʈ")]
    public List<UnitSlotController> unitSlots;
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
    public GameObject currentTurnSlotIcon;

    [SerializeField]
    [Header("SkillSlot")]
    [Tooltip("��ų ���� ����Ʈ")]
    public List<SkillSlotUIController> skillSlot;

    [SerializeField]
    [Tooltip("�� ������ ���� ��ġ�� ������ ��ųʸ�")]
    private SerializableDictionary<GameObject, Vector3> originalPositions = new SerializableDictionary<GameObject, Vector3>();

    // ���� ���� ��� ���� ������ ��ȣ
    public int currentTurnSlotNumber;

    // ������ �̵� �� ���¸� �Ǵ��ϴ� ��
    private bool isMoving = false;
    // ���� ����Ǿ������� �Ǵ��ϴ� ��
    private bool isTurnEnd = true;

    void Start()
    {
        Init();
    }

    /// <summary>
    /// ���� �ʱ� ������ �����մϴ�.
    /// </summary>
    private void Init()
    {
        SlotPosInit();
        ActionPointsInit();
        StartCoroutine(ActionPointAccumulation());
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
                setUnitSlots[i] = unitSlots[i + 4];
            }
        }
        else
        {
            for (int i = 0; setUnitSlots.Count > i; i++)
            {
                setUnitSlots[i] = unitSlots[i];
            }
        }
    }

    #region Turn

    /// <summary>
    /// �� unitSlot�� actionPoints�� �ʱ�ȭ�մϴ�.
    /// </summary>
    private void ActionPointsInit()
    {
        foreach (var unit in unitSlots)
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

    /// <summary>
    /// actionPoints�� ���������� �� ������ �ӵ���ŭ ������ŵ�ϴ�.
    /// </summary>
    private IEnumerator ActionPointAccumulation()
    {
        while (true)
        {
            if (isTurnEnd == true)
            {
                foreach (var unit in unitSlots)
                {
                    if (unit != null)
                    {
                        if (actionPoints.TryGetValue(unit, out float currentPoints))
                        {
                            actionPoints[unit] = currentPoints + unit.speed * Time.deltaTime;
                            unit.currentActionPoint = actionPoints[unit];


                            if (unit.slotNum >= 0 && unit.slotNum < 4)
                            {
                                float currentSpeed = unit.speed;
                                if (cost <= 10)
                                {
                                    cost += (currentSpeed / 20) * Time.deltaTime;
                                }
                                else if (cost > 10)
                                {
                                    cost = 10;
                                }
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"No key found for {unit.name}. Adding key.");
                            actionPoints.Add(unit, unit.speed * Time.deltaTime);  // Ű�� ���� ��� �߰�
                            unit.currentActionPoint = actionPoints[unit];
                        }
                    }
                }
                GetNextTurnSlotNumber();
            }
            yield return null;
        }
    }

    /// <summary>
    /// ���� ���� ������ ������ �����մϴ�.
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
    /// ������ ���� �����մϴ�.
    /// </summary>
    private void ExecuteTurn(UnitSlotController unit)
    {
        isTurnEnd = false;

        unit.currentTargetIcon.SetActive(true);
        currentTurnSlotNumber = unitSlots.IndexOf(unit);
        SkillSlotInit(unitSlots[currentTurnSlotNumber].skillDatas);

        UnitSlotController currentTurnSlot = unitSlots[currentTurnSlotNumber];
        currentTurnSlot.currentTargetIcon.SetActive(true);

        actionPoints[currentTurnSlot] = 0;
        Debug.Log("���� �����մϴ�. ���� ���� " + unitSlots[0].name + " (" + unitSlots[0].speed + " �ӵ�) �����Դϴ�.");
    }

    /// <summary>
    /// �� ���� �� ���� �ð� ��⸦ �����մϴ�.
    /// </summary>
    public IEnumerator DelayTurnEnd(float delay)
    {
        yield return new WaitForSeconds(delay); // ������ �ð���ŭ ���
        TurnEnd(); // ��� �� ȣ���� �Լ�
    }

    /// <summary>
    /// ���� �����մϴ�.
    /// </summary>
    public void TurnEnd()
    {
        unitSlots[currentTurnSlotNumber].SetAnim(0);
        unitSlots[currentTurnSlotNumber].currentTargetIcon.SetActive(false);
        isTurnEnd = true;
    }
    #endregion

    #region SlotMove
    private void SlotPosInit()
    {
        for (int i = 0; i < unitSlots.Count; i++)
        {
            if (unitSlots[i] != null)
            {
                var unit = unitSlots[i].gameObject;
                if (unit != null)
                {
                    // �ʱ� ���� ��ġ ����
                    originalPositions[unit] = unit.transform.position;
                }
            }
        }
    }

    /// <summary>
    /// ������ ������ �ٸ� ������ ��ġ�� �̵���ŵ�ϴ�.
    /// </summary>
    /// <param name="moveUnitNum">�̵��� ������ �ε����Դϴ�.</param>
    /// <param name="targetUnitNum">��ǥ ������ �ε����Դϴ�.</param>
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
            isMoving = false; // �̵� �Ϸ� �� �̵� �� ���� ����
        });
    }

    /// <summary>
    /// ���� ���� ���� ������ ������ �Ǵ� �������� �̵���ŵ�ϴ�.
    /// </summary>
    /// <param name="rightMove">���������� �̵����� �����Դϴ�. false�� �������� �̵��մϴ�.</param>
    public void DirectMoveUnit(bool rightMove)
    {
        int moveTargetUnitNum = currentTurnSlotNumber;

        if (isMoving || unitSlots[currentTurnSlotNumber] == null || unitSlots[moveTargetUnitNum] == null)
        {
            Debug.LogWarning("Movement is currently locked or invalid slot numbers provided.");
            return;
        }

        if (currentTurnSlotNumber >= 0 && currentTurnSlotNumber < 4)
        {
            if (rightMove)
            {
                if (currentTurnSlotNumber < 3)
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
    }

    #endregion

    #region Skill

    /// <summary>
    /// ��ų ������ �ʱ�ȭ�ϰ� �־��� ��ų �����ͷ� �����մϴ�.
    /// </summary>
    /// <param name="setSkillDatas">������ ��ų ������ ����Ʈ�Դϴ�.</param>
    public void SkillSlotInit(List<SkillData> setSkillDatas)
    {
        for (int i = 0; i < setSkillDatas.Count; i++)
        {
            skillSlot[i].SetSkillData(setSkillDatas[i]);
        }
    }

    /// <summary>
    /// ������ ������ �������� ������ �����մϴ�.
    /// </summary>
    /// <param name="attackRange">������ �����Դϴ�.</param>
    /// <param name="damage">������ �������Դϴ�.</param>
    public void ExecuteAttack(int attackRange, int damage)
    {
        unitSlots[currentTurnSlotNumber + attackRange].currentHp -= damage;
    }

    #endregion
}
