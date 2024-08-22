using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitSelectSlotUIController : MonoBehaviour
{
    [SerializeField]
    private UnitData unitData;


    [Header("Object")]
    [SerializeField]
    private TMP_Text unitName;
    [SerializeField]
    private Image unitIcon;
    [SerializeField]
    private List<Image> subIcons;


    public void Init()
    {
        unitName.text = unitData.name;
        unitIcon.sprite = unitData.unitFaceIcon;
    }
}
