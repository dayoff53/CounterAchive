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

    [Header("������ ��")]
    public int unitTeam = 0;

    /// <summary>
    /// ���� �ش� ���Կ� ��ġ�� ����
    /// </summary>
    [SerializeField]
    [Header("���� �ش� ���Կ� ��ġ�� ����")]
    public UnitBase unit;

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

    public void SetUnit(UnitStatus setUnitState)
    {
        unit.SetStatus(setUnitState);

        unit.SetAnim(0);
        unit.SetActionPointBar(0.0f);

        StatusInit();
    }
    /// <summary>
    /// �ش� ��ġ�� ���ο� ������ ��ġ
    /// </summary>
    /// <param name="setUnitState"></param>
    /// <param name="teamNum"></param>
    public void SetUnit(UnitStatus setUnitState, int teamNum)
    {
        SetUnit(setUnitState);
    }

    public void SetUnit(GameObject setUnit, int teamNum)
    {
        if (setUnit.GetComponent<UnitBase>() != null)
        {
            if (unit != null)
            {
                Destroy(unit.gameObject);
            }

            GameObject newUnit = Instantiate(setUnit, gameObject.transform);
            unit = newUnit.GetComponent<UnitBase>();
        }
        else
        {
            Debug.LogError("�ش� �����տ� UnitController ������Ʈ�� �����ϴ�.");
        }
    }
}
