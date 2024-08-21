using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class SkillSlotUIController : MonoBehaviour
{
    private SkillManager skillManager;

    [Header("SkillData")]
    public SkillData skillData;

    [Header("SkillUIBoxSetting")]
    public TMP_Text skillNameText;
    public TMP_Text skillFlavorText;
    public TMP_Text skillCostText;
    public Image skillIcon;
    public int skillCurrentDamage;
    public SkillRangeUIController skillRangeUIController;
    public Button skillButton;
    public UnitSlotController unitSlotController;
    public DataManager DataManager;



    void Start()
    {
        DataManager = DataManager.Instance;

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
            skillButton.onClick.AddListener(OnButtonClick);

            skillRangeUIController.SkillRangeInit(skillData.skillRange);
        }
    }

    public void SetSkillData(SkillData setSkillData)
    {
        skillData = setSkillData;

        Init();
    }


    private void OnButtonClick()
    {
        DataManager.currentTurn = turnState.SkillSelect;
        skillManager.currentSkillSlot = this;

        skillRangeUIController.SetRangeSprite(skillData.skillRange);
    }
}
