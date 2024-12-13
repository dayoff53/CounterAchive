using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StageMenuController : MonoBehaviour
{
    private StageManager stageManager;

    public Image skillIcon;
    public TMP_Text flavorText;
    public List<Image> areaSlotList;
    public List<Image> rangeSlotList;


    private void Start()
    {
        StageMenuInit();
    }
    public void StageMenuInit()
    {
        stageManager = StageManager.Instance;

        if (stageManager.currentSkillData)
        {
            skillIcon.sprite = stageManager.currentSkillData.skillIcon;
            flavorText.text = stageManager.currentSkillData.skillFlavorText;

            foreach (Image rangeSlot in rangeSlotList)
            {
                rangeSlot.color = stageManager.unitStateColors[0];
            }
            rangeSlotList[0].color = stageManager.unitStateColors[1];
            foreach (int rangeNum in stageManager.currentSkillData.skillRange)
            {
                rangeSlotList[rangeNum].color = stageManager.unitStateColors[2];
            }

            foreach (Image areaSlot in areaSlotList)
            {
                areaSlot.color = stageManager.unitStateColors[0];
            }
            foreach (int areaNum in stageManager.currentSkillData.skillArea)
            {
                areaSlotList[areaNum].color = stageManager.unitStateColors[2];
            }
        }
    }
}
