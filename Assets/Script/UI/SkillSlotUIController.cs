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
    [SerializeField]
    private SkillRangeUIController skillRangeUIController;
    [SerializeField]
    private Button skillButton;
    private StageManager stageManager;



    void Start()
    {
        stageManager = StageManager.Instance;

        Init();
    }

    // Start is called before the first frame update
    private void Init()
    {
        if (skillData != null)
        {
            skillIcon.sprite = skillData.skillIcon;
            skillNameText.text = skillData.skillName;
            skillFlavorText.text = skillData.skillFlavorText;
            skillCostText.text = skillData.skillCost.ToString();
            skillButton.onClick.AddListener(OnButtonClick);

            skillRangeUIController.SkillRangeInit(skillData.skillRange);
        }
        else
        {
            skillIcon.sprite = null;
            skillNameText.text = "";
            skillFlavorText.text = "";
            skillCostText.text = "";
            skillButton.onClick.AddListener(OnButtonClick);
            int[] skillRange = { };
            skillRangeUIController.SkillRangeInit(skillRange);
        }
    }

    public void SetSkillData(SkillData setSkillData)
    {
        skillData = setSkillData;

        Init();
    }


    private void OnButtonClick()
    {
        stageManager.currentPrograssState = ProgressState.SkillTargetSearch;
        stageManager.currentSkillSlot = this;

        skillRangeUIController.SetRangeGround(skillData.skillRange);
    }
}
