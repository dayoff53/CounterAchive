using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Sirenix.OdinInspector;

[DetailedInfoBox("인 게임 Stage에서 우측 하단에 위치하는 각종 스킬 설명 데이터들을 표기하는 공간의 컴포넌트", "현재 선택된 스킬의 데이터를 받아와 InfoUI에 적용하여 표기합니다.")]

public class SkillInfoUIController : MonoBehaviour
{
    [SerializeField]
    private BattleStageMaster stageManager;

    public Image skillIcon;
    public TMP_Text flavorText;
    public List<Image> areaSlotList;
    public List<Image> rangeSlotList;


    public void Init(BattleStageMaster stageManager)
    {
        this.stageManager = stageManager;
    }


    public void StageMenuRefresher()
    {
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
