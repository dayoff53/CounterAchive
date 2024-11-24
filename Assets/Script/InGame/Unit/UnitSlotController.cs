using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
public class UnitSlotController : MonoBehaviour
{
    /// <summary>
    /// 현재 위치한 Field의 UnitFieldController
    /// </summary>
    [SerializeField]
    [Header("현재 위치한 Field의 UnitFieldController")]
    UnitSlotGroupController unitFieldController;

    public int slotNumber;

    /// <summary>
    /// 현재 해당 슬롯에 위치한 유닛
    /// </summary>
    [SerializeField]
    [Header("현재 해당 슬롯에 위치한 유닛")]
    public UnitController unit;

    /// <summary>
    /// 유닛 슬롯이 비어있는지 여부
    /// </summary>
    [Header("유닛 슬롯이 비어있는지 여부")]
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
            Debug.LogError("해당 프리팹에 UnitController 컴포넌트가 없습니다.");
        }
    }
}
