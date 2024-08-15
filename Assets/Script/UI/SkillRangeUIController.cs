using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//스킬 사거리 색칠해주는 스크립트
public class SkillRangeUIController : MonoBehaviour
{
    [SerializeField]
    private List<Image> rangeImages;

    //스킬 슬롯의 사거리에 알맞게 사거리를 표시함
    public void rangeColorChange(List<int> skillRange)
    {
        for(int i = 0; i < rangeImages.Count; i++)
        {
            rangeImages[i].color = Color.white;
        }


        for(int i = 0; i < skillRange.Count; i++)
        {
            rangeImages[skillRange[i]].color = Color.blue;
        }
    }
}
