using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New GroupUnitsData", menuName = "Datas/GroupUnitsData", order = 0)]

public class GroupUnitsData : ScriptableObject
{
    public string groupName;
    public List<UnitData> groupUnitList;
}
