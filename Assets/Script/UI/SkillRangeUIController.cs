using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SkillSlot¿« UI
/// </summary>
public class SkillRangeUIController : MonoBehaviour
{
    [SerializeField]
    private List<Image> rangeImages;

    [SerializeField]
    private List<Color> skillRangeColors;

    [SerializeField]
    private StageManager stageManager;


    private void Start()
    {
        stageManager = StageManager.Instance;
    }

    public void SkillRangeInit(int[] skillRange)
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
