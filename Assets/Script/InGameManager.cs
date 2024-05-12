using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class InGameManager : Singleton<InGameManager>
{
    // 현재 턴을 행사중인 슬롯의 번호
    public int currentTurnSlotNumber;

    public float cost;

    [Header("UnitSlot")]
    [Tooltip("유닛 슬롯 리스트")]
    public List<UnitSlotController> unitSlots;
    [Tooltip("턴 순서 리스트")]
    public List<UnitSlotController> turnOrder;
    [Tooltip("행동력 누적을 위한 Dictionary")]
    private Dictionary<UnitSlotController, float> actionPoints = new Dictionary<UnitSlotController, float>();


    public List<float> currentTurn;


    [Tooltip("현재 턴을 지닌 유닛 식별표")]
    public GameObject currentTurnSlotIcon;


    [Header("SkillSlot")]
    [Tooltip("스킬 슬롯 리스트")]
    public List<SkillSlotUIController> skillSlot;


    // 각 슬롯의 원래 위치를 저장할 딕셔너리
    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();

    // 유닛의 이동 중 상태를 판단하는 값
    private bool isMoving = false;

    void Start()
    {
        Init();
    }

    private void Init()
    {
        SlotPosInit();
        TurnCycle();
    }
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
    // 턴 순서를 계산 후 턴을 넘김
    private void TurnCycle()
    {
        turnOrder = unitSlots.OrderBy(u => -u.unitData.speed).ToList();

        currentTurnSlotNumber = unitSlots.IndexOf(turnOrder[0]);
        cost += unitSlots[currentTurnSlotNumber].unitData.speed;
        SkillSlotInit(unitSlots[currentTurnSlotNumber].unitData.skillDatas);

        SetTurnSlot();

        Debug.Log("턴을 시작합니다. 현재 턴은 " + unitSlots[0].name + " (" + unitSlots[0].unitData.speed + " 속도) 유닛입니다.");
    }

    // 특정 슬롯에게 턴을 넘김
    public void SetTurnSlot()
    {
        // 현재 턴 슬롯의 GameObject를 찾습니다.
        UnitSlotController currentTurnSlot = unitSlots[currentTurnSlotNumber];

        currentTurnSlot.currentTargetIcon.SetActive(true);
    }

    public IEnumerator DelayTurnEnd(float delay)
    {
        yield return new WaitForSeconds(delay); // 지정된 시간만큼 대기
        TurnEnd(); // 대기 후 호출할 함수
    }

    // 턴을 종료시킴
    public void TurnEnd()
    {
        unitSlots[currentTurnSlotNumber].SetAnim(0);
        unitSlots[currentTurnSlotNumber].currentTargetIcon.SetActive(false);

        TurnCycle();
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
                    // 초기 유닛 위치 설정
                    originalPositions[unit] = unit.transform.position;
                }
            }
        }
    }


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

    public void SkillSlotInit(List<SkillData> setSkillDatas)
    {
        for (int i = 0; i < setSkillDatas.Count; i++)
        {
            skillSlot[i].SetSkillData(setSkillDatas[i]);
        }
    }

    public void ExecuteAttack(int attackRange, int damage)
    {
        unitSlots[currentTurnSlotNumber + attackRange].unitData.hp -= damage;
    }

    #endregion
}
