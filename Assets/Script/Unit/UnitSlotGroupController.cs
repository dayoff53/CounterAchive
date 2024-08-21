using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UnitSlotGroupController : MonoBehaviour
{
    [SerializeField]
    private List<UnitSlotController> unitSlots;

    [SerializeField]
    private int currentSlotNum;

    // 각 슬롯의 원래 위치를 저장할 딕셔너리
    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();

    // 유닛의 이동 중 상태를 판단하는 값
    private bool isMoving = false;
    DataManager inGameManager;


    public void Awake()
    {
        inGameManager = DataManager.Instance;
        inGameManager.SetUnitSlot(unitSlots);
    }

    public void Start()
    {
        Init();
    }

    private void Init()
    {
        for (int i = 0; i < unitSlots.Count; i++)
        {
            UnitSlotController unit = unitSlots[i];

            if (unit != null)
            {
                var unitObject = unit.gameObject;
                if (unitObject != null)
                {
                    // 초기 유닛 위치 설정
                    originalPositions[unitObject] = unitObject.transform.position;
                }

                //현재 유닛 데이터 적용
                unit.slotNum = i;
                unit.slotGroundSpriteController.SetSlotGroundState(SlotGroundState.Normal, inGameManager.unitStateColors[0]);
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
            unitSlots[moveUnitNum].slotNum = targetUnitNum;
            unitSlots[targetUnitNum].slotNum = moveUnitNum;


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

}
