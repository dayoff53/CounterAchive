using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class SkillSlotUIController : MonoBehaviour
{
    [Header("SkillData")]
    public SkillData skillData;

    [Header("SkillUIBoxSetting")]
    public TMP_Text skillNameText;
    public TMP_Text skillFlavorText;
    public TMP_Text skillCostText;
    public Image skillIcon;
    public SkillRangeUIController skillRangeUIController;
    public Button skillButton;
    SkillManager skillManager;



    void Start()
    {
        skillManager = SkillManager.Instance;

        Init();
    }

    // Start is called before the first frame update
    private void Init()
    {
        if (skillData)
        {
            skillIcon.sprite = skillData.skillIcon;
            skillNameText.text = skillData.skillName;
            skillFlavorText.text = skillData.skillFlavorText;
            skillCostText.text = skillData.skillCost.ToString();
            skillRangeUIController.rangeColorChange(skillData.skillRange);
            skillButton.onClick.AddListener(OnButtonClick);
        }
    }

    private void OnButtonClick()
    {
        //skillManager.ExecuteSkill(skillData, )
    }

    public void SetSkillData(SkillData setSkillData)
    {
        skillData = setSkillData;

        Init();
    }
}
