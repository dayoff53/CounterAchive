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

    //UnitSelectSlot�� ó�� ȣ�� �Ǿ��� ��� Null������ �� ��, ���� �����Ͱ� ����Ǿ����� �� �� ȣ�� ��
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
