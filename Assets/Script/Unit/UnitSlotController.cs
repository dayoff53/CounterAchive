using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSlotController : MonoBehaviour
{
    [SerializeField]
    [Header("���� ��ġ�� Field�� UnitFieldController")]
    UnitFieldMoveController unitFieldController;

    [SerializeField]
    [Header("SlotNum")]
    public int slotNum = 0;

    [SerializeField]
    [Header("���� �ش� ���Կ� ��ġ�� ����")]
    public UnitData unitData;

    [SerializeField]
    public SpriteRenderer sprite;

    [SerializeField]
    public Animator unitAnimator;


    [SerializeField]
    public GameObject currentTargetIcon;

    public UnitData GetUnitData()
    {
        return unitData;
    }

    public void SetAnim(int animNum)
    {
        unitAnimator.SetInteger("State", animNum);
    }

    private void Init()
    {

    }
}
