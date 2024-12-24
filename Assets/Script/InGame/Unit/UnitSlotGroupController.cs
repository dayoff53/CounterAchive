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

    // �� ������ ���� ��ġ�� ������ ��ųʸ�
    private Dictionary<int, Vector3> originalPositions = new Dictionary<int, Vector3>();

    StageManager gameManager;



    public void Start()
    {
        gameManager = StageManager.Instance;
    }

    /// <summary>
    /// GameManager�� �ش� ��ũ��Ʈ�� ����ϱ� �ռ� �ʱ�ȭ�ϴ� �۾�
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
                    // �ʱ� ���� ������Ʈ�� ��ġ ����
                    originalPositions[i] = unitObject.transform.position;
                }

                unitSlots[i].slotGround. SetSlotGroundState(SlotGroundState.Default);
            }

            unitSlots[i].UnitStatusInit();
        }
        gameManager.unitSlotList = unitSlots;
    }

    /// <summary>
    /// �� Unit���� ��ġ�� �����մϴ�. �� ��� ��� Unit�� Ground�� Normal���·� ����˴ϴ�.
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
