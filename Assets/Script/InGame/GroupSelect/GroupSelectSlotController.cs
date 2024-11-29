using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GroupSelectSlotController : MonoBehaviour
{
    /// <summary>
    /// 선택한 그룹의 유닛 데이터
    /// </summary>
    [SerializeField]
    public GroupUnitsData groupUnitsData;

    [SerializeField]
    private GameObject groupSelectCheckUIObject;
    [SerializeField]
    private Button groupSelectCheckButton;

    private DataManager dataManager;

    private void Start()
    {
        dataManager = DataManager.Instance;
    }

    /// <summary>
    /// 그룹 선택 창 에서 플레이 할 그룹 선택 시 확인 여부 UI를 출력시킵니다.
    /// </summary>
    public void OnGroupSelectButton()
    {
        groupSelectCheckUIObject.SetActive(true);
        dataManager.playerUnitStateList = new List<UnitStatus>();

        for (int i = 0; i < groupUnitsData.groupUnitList.Count; i++)
        {
            UnitData data;
            data = groupUnitsData.groupUnitList[i];
            UnitStatus state = new UnitStatus(data);

            dataManager.playerUnitStateList.Add(state);
        }
    }
}
