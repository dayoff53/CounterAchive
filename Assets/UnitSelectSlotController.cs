using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitSelectSlotController : MonoBehaviour
{


    [SerializeField]
    public UnitState unitState;

    private StageManager gameManager;
    private DataManager dataManager;

    [Header("Object")]
    [SerializeField]
    private TMP_Text unitName;
    [SerializeField]
    private Image unitIcon;
    [SerializeField]
    private List<Image> subIcons;

    private void Start()
    {
        Init();
    }

    //UnitSelectSlot�� ó�� ȣ�� �Ǿ��� ��� Null������ �� ��, ���� �����Ͱ� ����Ǿ����� �� �� ȣ�� ��
    public void Init()
    {
        gameManager = StageManager.Instance;
        dataManager = DataManager.Instance;

        unitName.text = unitState.unitName;
        unitIcon.sprite = dataManager.unitDataList.Find(unit => unit.unitNumber == unitState.unitNumber).unitFaceIcon;


        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }


    private void OnButtonClick()
    {
        //gameManager.currentPrograssState = ProgressState.UnitSelect;
        gameManager.currentSelectUnitState = unitState;
    }
}
