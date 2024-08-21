using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New ColorState", menuName = "ColorState", order = 0)]
[System.Serializable]


public class ColorState : ScriptableObject
{
    public List<Color> colorStates;
}
