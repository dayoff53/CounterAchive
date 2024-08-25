using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitSelectSlotUIController : MonoBehaviour
{
    [SerializeField]
    private UnitData unitData;


    [SerializeField]
    private GameManager gameManager;

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

    public void Init()
    {
        unitName.text = unitData.name;
        unitIcon.sprite = unitData.unitFaceIcon;
        gameManager = GameManager.Instance;


        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }


    private void OnButtonClick()
    {
        //gameManager.currentPrograssState = ProgressState.UnitSelect;
        gameManager.currentSelectUnitData = unitData;
    }
}
