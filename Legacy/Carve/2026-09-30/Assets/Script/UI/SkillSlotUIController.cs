using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class SkillSlot : MonoBehaviour
{
    [Header("SkillData")]
    public SkillData skillData;

    [Header("SkillUIBoxSetting")]
    public TMP_Text skillNameText;
    public TMP_Text skillFlavorText;
    public TMP_Text skillCostText;
    public Image skillIcon;
    [SerializeField]
    private SkillRangeUIController skillRangeUIController;
    [SerializeField]
    private Button skillButton;
    private BattleStageMaster stageManager;



    private void Start()
    {
        Init();
    }

    public void Init(BattleStageMaster stageManager)
    {
        this.stageManager = stageManager;
        skillData = null;
        Init();
    }

    private void Init()
    {
        skillButton.onClick.RemoveListener(OnButtonClick);

        if (skillData != null)
        {
            skillIcon.sprite = skillData.skillIcon;
            skillNameText.text = skillData.skillName;
            skillFlavorText.text = skillData.skillFlavorText;
            skillCostText.text = skillData.skillCost.ToString();
            skillButton.onClick.AddListener(OnButtonClick);

            skillRangeUIController.Init(skillData.skillRange);
        }
        else
        {
            skillIcon.sprite = null;
            skillNameText.text = "";
            skillFlavorText.text = "";
            skillCostText.text = "";
            skillButton.onClick.AddListener(OnButtonClick);
            int[] skillRange = { };
            skillRangeUIController.Init(skillRange);
        }
    }

    public void SetSkillData(SkillData setSkillData)
    {
        if (setSkillData != null)
            skillData = setSkillData;
        else
            skillData = null;

        Init();
    }


    private void OnButtonClick()
    {
        if (skillData != null)
            stageManager.SkillTypeSelect(skillData);
    }
}
