using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillRangeUIController : MonoBehaviour
{
    [SerializeField]
    private List<Image> rangeImages;


    public void rangeColorChange(int minRange, int maxRange)
    {
        for(int i = 0; i < rangeImages.Count; i++)
        {
            rangeImages[i].color = Color.white;
        }


        for(int currentRange = minRange; currentRange <= maxRange; currentRange++)
        {
            rangeImages[currentRange].color = Color.blue;
        }
    }
}
