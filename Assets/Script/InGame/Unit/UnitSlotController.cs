using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
public class UnitSlotController : MonoBehaviour
{
    /// <summary>
    /// ���� ��ġ�� Field�� UnitFieldController
    /// </summary>
    [SerializeField]
    [Header("���� ��ġ�� Field�� UnitFieldController")]
    UnitSlotGroupController unitFieldController;

    public int slotNumber;

    /// <summary>
    /// ���� �ش� ���Կ� ��ġ�� ����
    /// </summary>
    [SerializeField]
    [Header("���� �ش� ���Կ� ��ġ�� ����")]
    public UnitController unit;

    /// <summary>
    /// ���� ������ ����ִ��� ����
    /// </summary>
    [Header("���� ������ ����ִ��� ����")]
    public bool isNull = false;

    [SerializeField]
    public SlotGroundSpriteController slotGround;


    public void StatusInit()
    {
        if (unit.unitData.name == "Null")
        {
            isNull = true;
        }
        else
        {
            isNull = false;
        }

        unit.StatusInit();
    }

    public void SetUnit(UnitState setUnitState)
    {
        unit.SetState(setUnitState);

        unit.SetAnim(0);
        unit.SetActionPointBar(0.0f);

        StatusInit();
    }

    public void SetUnit(UnitState setUnitState, int teamNum)
    {
        unit.unitTeam = teamNum;

        SetUnit(setUnitState);
    }

    public void SetUnit(GameObject setUnit, int teamNum)
    {
        if (setUnit.GetComponent<UnitController>() != null)
        {
            if (unit != null)
            {
                Destroy(unit.gameObject);
            }

            GameObject newUnit = Instantiate(setUnit, gameObject.transform);
            unit = newUnit.GetComponent<UnitController>();
        }
        else
        {
            Debug.LogError("�ش� �����տ� UnitController ������Ʈ�� �����ϴ�.");
        }
    }
}