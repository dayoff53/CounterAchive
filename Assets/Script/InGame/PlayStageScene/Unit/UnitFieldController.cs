using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitFieldController : MonoBehaviour
{
    [SerializeField]
    public List<UnitSlotController> unitSlots;

    [SerializeField]
    public int currentSlotNum;
}
