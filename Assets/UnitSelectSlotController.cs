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

    //UnitSelectSlot이 처음 호출 되었을 경우 Null값으로 한 번, 유닏 데이터가 변경되었을때 한 번 호출 됨
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
