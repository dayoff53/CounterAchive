using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// �÷��̾ ��ġ ������ ������ �����ϴ� ������ �Ѱ��ϴ� ��ũ��Ʈ
/// </summary>
public class UnitSelectSlotGroupController : MonoBehaviour
{
    [SerializeField]
    private GameObject selectUnitSlot;
    [SerializeField]
    private GameObject contentObject;

    /// <summary>
    /// ��ġ�� �� �ִ� UnitSlot�� List
    /// </summary>
    public List<UnitSelectSlotController> selectUnitSlotList;
    private DataManager dataManager;

    /// <summary>
    /// �÷��̾ ��ġ ������ ������ ���Ե��� �ʱ�ȭ�ϴ� ��ũ��Ʈ
    /// </summary>
    public void InitUnitSelectSlot()
    {
        dataManager = DataManager.Instance;
        selectUnitSlotList = new List<UnitSelectSlotController>();

        Debug.Log($"dataManager.playerUnitStateList.Count : {dataManager.playerUnitStateList.Count}");
        for (int i = 0; i < dataManager.playerUnitStateList.Count; i++)
        {
            GameObject currentSelectUnitSlot = Instantiate(selectUnitSlot, contentObject.transform);
            selectUnitSlotList.Add(currentSelectUnitSlot.GetComponent<UnitSelectSlotController>());
            selectUnitSlotList[i].unitState = dataManager.playerUnitStateList[i];
            selectUnitSlotList[i].Init();

            Debug.Log($"selectUnitSlotList.Count : {selectUnitSlotList.Count}");
        }
    }
}
