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
    public InGameManager inGameManager;



    void Start()
    {
        inGameManager = InGameManager.Instance;

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

    private void OnButtonClick()
    {
        inGameManager.currentTurn = turnState.SkillSelect;
        inGameManager.currentSkillSlot = this;
        skillRangeUIController.SetRangeSprite(skillData.skillRange);
    }

    public void SetSkillData(SkillData setSkillData)
    {
        skillData = setSkillData;

        Init();
    }

    public void OnSkillPlay(int targetNum)
    {
        inGameManager.cost -= skillData.skillCost;
        inGameManager.ExecuteAttack(targetNum, inGameManager.currentSkillSlot.skillData.damage);
    }
}
