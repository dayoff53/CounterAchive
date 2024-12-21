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

    [Header("유닛의 팀")]
    public int unitTeam = 0;

    /// <summary>
    /// 현재 해당 슬롯에 위치한 유닛
    /// </summary>
    [SerializeField]
    [Header("현재 해당 슬롯에 위치한 유닛")]
    public UnitBase unit;

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

    public void SetUnit(UnitStatus setUnitState)
    {
        unit.SetStatus(setUnitState);

        unit.SetAnim(0);
        unit.SetActionPointBar(0.0f);

        StatusInit();
    }
    /// <summary>
    /// 해당 위치에 새로운 유닛을 배치
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
            Debug.LogError("해당 프리팹에 UnitController 컴포넌트가 없습니다.");
        }
    }
}
