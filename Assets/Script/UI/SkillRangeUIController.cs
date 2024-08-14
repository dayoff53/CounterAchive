using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//��ų ��Ÿ� ��ĥ���ִ� ��ũ��Ʈ
public class SkillRangeUIController : MonoBehaviour
{
    [SerializeField]
    private List<Image> rangeImages;

    //��ų ������ ��Ÿ��� �˸°� ��Ÿ��� ǥ����
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
