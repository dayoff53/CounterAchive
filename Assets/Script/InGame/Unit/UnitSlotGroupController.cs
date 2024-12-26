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

    // 각 슬롯의 원래 위치를 저장하는 딕셔너리
    private Dictionary<int, Vector3> originalPositions = new Dictionary<int, Vector3>();

    StageManager gameManager;



    public void Start()
    {
        gameManager = StageManager.Instance;
    }

    /// <summary>
    /// GameManager에서 현재 유닛 슬롯을 초기화하고 슬롯 상태를 초기화하는 메서드
    /// </summary>
    public void UnitSlotsInit()
    {
        gameManager = StageManager.Instance;
        for (int i = 0; i < unitSlots.Count; i++)
        {
            if (unitSlots[i] != null)
            {
                GameObject unitObject = unitSlots[i].gameObject;
                if (unitObject != null)
                {
                    // 모든 슬롯 위치 초기화
                    originalPositions[i] = unitObject.transform.position;
                }

                unitSlots[i].slotGround.SetSlotGroundState(SlotGroundState.Default);
            }

            unitSlots[i].UnitStatusInit();
        }
        gameManager.unitSlotList = unitSlots;
    }

    /// <summary>
    /// 유닛의 위치를 원래 위치로 이동시킵니다. 이동 후 애니메이션을 적용합니다.
    /// </summary>
    public void MoveUnit()
    {
        for (int i = 0; i < unitSlots.Count; i++)
        {
            GameObject moveSlot = unitSlots[i].gameObject;


            Tween tween = DOTween.Sequence();
            tween = moveSlot.transform.DOMove(originalPositions[i], 1f).SetEase(Ease.InOutQuad);
            tween.OnComplete(() =>
            {
                moveSlot.transform.position = originalPositions[i];
            });
        }
    }

    public void EnemyUnitInit(List<UnitStatus> enemyUnitSlots)
    {
        int endNum = unitSlots.Count - 1;

        for (int i = 0; i < enemyUnitSlots.Count; i++)
        {
            unitSlots[endNum - i].SetUnit(enemyUnitSlots[i], 2);
        }
    }

    public void PlayerUnitInit(List<UnitStatus> playerUnitSlots)
    {
        int startNum = 0;

        for (int i = 0; i < playerUnitSlots.Count; i++)
        {
            unitSlots[startNum + i].SetUnit(playerUnitSlots[i], 1);
        }
    }
}
