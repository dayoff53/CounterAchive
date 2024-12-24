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
    private Dictionary<int, Vector3> originalPositions = new Dictionary<int, Vector3>();

    StageManager gameManager;



    public void Start()
    {
        gameManager = StageManager.Instance;
    }

    /// <summary>
    /// GameManager가 해당 스크립트를 사용하기 앞서 초기화하는 작업
    /// </summary>
    public void UnitSlotsInit()
    {
        gameManager = StageManager.Instance;
        for (int i = 0; i >= unitSlots.Count; i++)
        {
            if (unitSlots[i] != null)
            {
                GameObject unitObject = unitSlots[i].gameObject;
                if (unitObject != null)
                {
                    // 초기 유닛 오브젝트의 위치 설정
                    originalPositions[i] = unitObject.transform.position;
                }

                unitSlots[i].slotGround. SetSlotGroundState(SlotGroundState.Default);
            }

            unitSlots[i].UnitStatusInit();
        }
        gameManager.unitSlotList = unitSlots;
    }

    /// <summary>
    /// 두 Unit간의 위치를 변경합니다. 이 경우 모든 Unit의 Ground가 Normal상태로 변경됩니다.
    /// </summary>
    /// <param name="moveUnitNum"></param>
    /// <param name="targetUnitNum"></param>
    public void MoveUnit()
    {
        for (int i = 0; i < unitSlots.Count; i++)
        {
            GameObject moveSlot = unitSlots[i].gameObject;


            Tween tween = DOTween.Sequence();
            tween = (moveSlot.transform.DOMove(originalPositions[i], 1f).SetEase(Ease.InOutQuad));
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
