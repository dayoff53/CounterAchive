using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UnitSlotGroupController : MonoBehaviour
{
    [SerializeField]
    public List<UnitSlotController> unitSlots;

    [SerializeField]
    private int currentSlotNum;

    // 각 슬롯의 원래 위치를 저장할 딕셔너리
    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();

    // 유닛의 이동 중 상태를 판단하는 값
    private bool isMoving = false;
    StageManager gameManager;



    public void Start()
    {
        gameManager = StageManager.Instance;
        //UnitSlotsInit();
    }

    /// <summary>
    /// GameManager가 해당 스크립트를 사용하기 앞서 초기화하는 작업
    /// </summary>
    public void UnitSlotsInit()
    {
        gameManager = StageManager.Instance;
        for (int i = 0; i < unitSlots.Count; i++)
        {
            UnitSlotController unitSlot = unitSlots[i];

            if (unitSlot != null)
            {
                var unitObject = unitSlot.gameObject;
                if (unitObject != null)
                {
                    // 초기 유닛 위치 설정
                    originalPositions[unitObject] = unitObject.transform.position;
                }

                if(unitSlot.unit.unitTeam == 2)
                {
                    unitSlot.unit.SetDirection(true);
                }

                unitSlot.slotNumber = i;
                unitSlot.slotGround.SetSlotGroundState(SlotGroundState.Default);
            }

            unitSlot.StatusInit();
        }
        gameManager.unitSlotList = unitSlots;
    }

    /// <summary>
    /// 두 Unit간의 위치를 변경합니다. 이 경우 모든 Unit의 Ground가 Normal상태로 변경됩니다.
    /// </summary>
    /// <param name="moveUnitNum"></param>
    /// <param name="targetUnitNum"></param>
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
            unitSlots[moveUnitNum].slotNumber = targetUnitNum;
            unitSlots[targetUnitNum].slotNumber = moveUnitNum;


            Debug.Log("Units have been swapped successfully.");
            isMoving = false; // 이동 완료 후 이동 중 상태 해제
        });

    }

    public void DirectMoveUnit(bool rightMove)
    {
        int moveTargetUnitNum = currentSlotNum;

        if (isMoving || unitSlots[currentSlotNum] == null || unitSlots[moveTargetUnitNum] == null)
        {
            Debug.LogWarning("Movement is currently locked or invalid slot numbers provided.");
            return;
        }


        if (rightMove)
        {
            if (currentSlotNum > 0)
            {
                MoveUnit(currentSlotNum, currentSlotNum - 1);

                currentSlotNum = currentSlotNum - 1;
            }
        }
        else
        {
            if (currentSlotNum < 3)
            {
                MoveUnit(currentSlotNum, currentSlotNum + 1);

                currentSlotNum = currentSlotNum + 1;
            }
        }
    }

    public void EnemyUnitInit(List<UnitState> enemyUnitSlots)
    {
        int endNum = unitSlots.Count - 1;

        for (int i = 0; i < enemyUnitSlots.Count; i++)
        {
            unitSlots[endNum - i].SetUnit(enemyUnitSlots[i], 2);
        }
    }
    public void PlayerUnitInit(List<UnitState> playerUnitSlots)
    {
        int startNum = 0;

        for (int i = 0; i < playerUnitSlots.Count; i++)
        {
            unitSlots[startNum + i].SetUnit(playerUnitSlots[i], 1);
        }
    }
}
