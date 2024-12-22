using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitSelectController : MonoBehaviour
{
    [SerializeField]
    public UnitStatus unitStatus;

    private StageManager gameManager;

    private void Start()
    {
        Init();
    }

    //UnitSelectSlot이 처음 호출 되었을 경우 Null값으로 한 번, 유닏 데이터가 변경되었을때 한 번 호출 됨
    public void Init()
    {
        gameManager = StageManager.Instance;

        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }


    private void OnButtonClick()
    {
        //gameManager.currentPrograssState = ProgressState.UnitSelect;
        gameManager.currentSelectUnitState = unitStatus;
    }
}
