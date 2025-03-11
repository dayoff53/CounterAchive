using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SkillSlot의 스킬 사거리를 표시하는 UI 컨트롤러 (지속해서 사용할지는 미지수)
/// </summary>
public class SkillRangeUIController : MonoBehaviour
{
    [SerializeField]
    private List<Image> rangeImages;

    [SerializeField]
    private List<Color> skillRangeColors;

    [SerializeField]
    private BattleStageMaster stageManager;


    public void Init(int[] skillRange, BattleStageMaster stageManager)
    {
        this.stageManager = stageManager;

        Init(skillRange);
    }

    public void Init(int[] skillRange)
    {
        for (int i = 0; i < rangeImages.Count; i++)
        {
            rangeImages[i].color = skillRangeColors[0];
        }

        rangeImages[0].color = skillRangeColors[1];

        for (int i = 0; i < skillRange.Length; i++)
        {
            rangeImages[skillRange[i]].color = skillRangeColors[2];
        }
    }
}
