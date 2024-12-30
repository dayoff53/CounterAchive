using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class UnitSlotController : MonoBehaviour
{
    /// <summary>
    /// 슬롯이 속한 필드의 UnitFieldController
    /// </summary>
    [Header("슬롯이 속한 필드의 UnitFieldController")]
    UnitSlotGroupController unitFieldController;

        // Start of Selection
        [Header("유닛 팀")]
        [SerializeField]
        private int _unitTeam = 0;
        public int unitTeam
        {
            get => _unitTeam;
            set
            {
                if (_unitTeam != value)
                {
                    _unitTeam = value;
                    Debug.Log($"unitTeam이 {_unitTeam}(으)로 변경되었습니다.");
                }
            }
        }

    /// <summary>
    /// 유닛의 부모 오브젝트
    /// </summary>
    [SerializeField]
    [Header("유닛의 부모 오브젝트")]
    public GameObject unitParent;

    /// <summary>
    /// 현재 슬롯에 있는 유닛의 참조
    /// </summary>
    [SerializeField]
    [Header("현재 슬롯에 있는 유닛의 참조")]
    public UnitBase unit;

    /// <summary>
    /// 유닛의 존재 여부
    /// </summary>
    [Header("유닛의 존재 여부")]
    public bool isNull = false;

    [SerializeField]
    public SlotGroundSpriteController slotGround;


    public void UnitStatusInit()
    {
        TurnEndInit();
        unit.UnitBaseUpdate();
    }

    public void TurnEndInit()
    {
        if (unit.unitData.name == "Null")
        {
            isNull = true;
            unitTeam = 0;
            unit.UnitBaseUpdate();
        }
        else
        {
            isNull = false;
        }
    }

    /// <summary>
    /// 현재 슬롯에 있는 유닛의 상태를 설정
    /// </summary>
    /// <param name="setUnitStatus">설정할 유닛 상태</param>
    public void SetUnit(UnitStatus setUnitStatus)
    {
        unit.SetStatus(setUnitStatus);
        
        UnitStatusInit();
    }

    /// <summary>
    /// 현재 슬롯에 있는 유닛의 상태와 팀 번호를 설정
    /// </summary>
    /// <param name="setUnitStatus">설정할 유닛 상태</param>
    /// <param name="teamNum">팀 번호</param>
    public void SetUnit(UnitStatus setUnitStatus, int teamNum)
    {
        unitTeam = teamNum;
        SetUnit(setUnitStatus);
    }

    /// <summary>
    /// 지정된 유닛 오브젝트로 현재 슬롯의 유닛을 설정
    /// </summary>
    /// <param name="setUnit">설정할 유닛 게임 오브젝트</param>
    /// <param name="teamNum">팀 번호</param>
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
            Debug.LogError("현재 오브젝트에 UnitBase 컴포넌트가 추가된 유닛을 설정해야 합니다.");
        }
    }
}
