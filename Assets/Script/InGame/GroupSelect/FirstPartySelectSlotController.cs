using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FirstPartySelectSlotController : MonoBehaviour
{
    /// <summary>
    /// ������ �׷��� ���� ������
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
    /// �׷� ���� â ���� �÷��� �� �׷� ���� �� Ȯ�� ���� UI�� ��½�ŵ�ϴ�.
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
