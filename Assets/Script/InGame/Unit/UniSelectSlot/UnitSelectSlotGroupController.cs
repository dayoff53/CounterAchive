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
    public List<UnitCard> selectUnitCardList;
    public List<UnitSelectController> selectUnitSelectList;
    private DataManager dataManager;

    /// <summary>
    /// �÷��̾ ��ġ ������ ������ ���Ե��� �ʱ�ȭ�ϴ� ��ũ��Ʈ
    /// </summary>
    public void InitUnitSelectSlot()
    {
        dataManager = DataManager.Instance;
        selectUnitCardList = new List<UnitCard>();
        selectUnitSelectList = new List<UnitSelectController>();

        Debug.Log($"dataManager.playerUnitStateList.Count : {dataManager.playerUnitStateList.Count}");
        for (int i = 0; i < dataManager.playerUnitStateList.Count; i++)
        {
            GameObject currentSelectUnitSlot = Instantiate(selectUnitSlot, contentObject.transform);

            selectUnitCardList.Add(currentSelectUnitSlot.GetComponent<UnitCard>());
            selectUnitCardList[i].unitStatus = dataManager.playerUnitStateList[i];
            selectUnitCardList[i].InitUnitCard();

            selectUnitSelectList.Add(currentSelectUnitSlot.AddComponent<UnitSelectController>());
            selectUnitSelectList[i].unitStatus = dataManager.playerUnitStateList[i];
            selectUnitSelectList[i].Init();

            Debug.Log($"selectUnitSlotList.Count : {selectUnitCardList.Count}");
        }
    }
}
