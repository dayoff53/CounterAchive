using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UnitCard : MonoBehaviour
{
    private DataManager dataManager;

    [SerializeField]
    private UnitStatus _unitStatus;
    public UnitStatus unitStatus
    {
        get
        {
            return _unitStatus;
        }
        set
        {
            _unitStatus = value;
            InitUnitCard();
        }
    }

    [System.Serializable]
    struct FrontPage
    {
        [Header("ī�� �ո�")]
        [Tooltip("������ �� ������")]
        public Image unitCardIcon;

        [Tooltip("������ �̸� �ؽ�Ʈ")]
        public TMP_Text unitCardName;

        [Tooltip("������ Hp ������")]
        public Image currentTurnHpGaugeBar;

        [Tooltip("������ Hp �ؽ�Ʈ")]
        public TMP_Text currentTurnHpText;
    }

    [System.Serializable]
    struct BackPage
    {
        [Header("ī�� �ո�")]
        [Tooltip("������ �� ������")]
        public Image unitCardIcon;

        [Tooltip("������ �̸� �ؽ�Ʈ")]
        public TMP_Text unitCardName;

        [Tooltip("������ Hp ������")]
        public Image hpGaugeBar;

        [Tooltip("������ Hp �ؽ�Ʈ")]
        public TMP_Text hpText;
        [Tooltip("������ Atk �ؽ�Ʈ")]
        public TMP_Text atkText;
        [Tooltip("������ Def �ؽ�Ʈ")]
        public TMP_Text defText;
        [Tooltip("������ Acc �ؽ�Ʈ")]
        public TMP_Text accText;
        [Tooltip("������ Eva �ؽ�Ʈ")]
        public TMP_Text evaText;
        [Tooltip("������ Speed �ؽ�Ʈ")]
        public TMP_Text speedText;

        [Tooltip("������ Tag �ؽ�Ʈ")]
        public TMP_Text tagText;
        public UnitTag_Native unitNative;
        public List<UnitTag> unitTagList;
    }

    [SerializeField]
    private FrontPage frontPage;
    [SerializeField]
    private GameObject frontPageObject;
    [SerializeField]
    private BackPage backPage;
    [SerializeField]
    private GameObject backPageObject;
    public bool isFront;


    // Start is called before the first frame update
    void Start()
    {
        dataManager = DataManager.Instance;
        InitUnitCard();
    }

    public void InitUnitCard()
    {
        if(dataManager == null)
            dataManager = DataManager.Instance;

        Debug.Log($"InitUnitCard : {gameObject.name}");
        frontPageObject.SetActive(true);
        backPageObject.SetActive(true);

        if (unitStatus.unitNumber != 0)
        {
            frontPage.unitCardIcon.color = Color.white;
            frontPage.unitCardIcon.sprite = dataManager.unitDataList.Find(u => u.unitNumber == unitStatus.unitNumber).unitFaceIcon;
            frontPage.unitCardName.text = unitStatus.unitName;

            backPage.unitCardIcon.color = Color.white;
            backPage.unitCardIcon.sprite = dataManager.unitDataList.Find(u => u.unitNumber == unitStatus.unitNumber).unitFaceIcon;
            backPage.unitCardName.text = unitStatus.unitName;
            backPage.hpText.text = $"{unitStatus.currentHp.ToString()}/{unitStatus.maxHp.ToString()}";
            backPage.atkText.text = $"Atk : {unitStatus.atk.ToString()}";
            backPage.defText.text = $"Def : {unitStatus.def.ToString()}";
            backPage.accText.text = $"Acc : {unitStatus.acc.ToString()}";
            backPage.evaText.text = $"Eva : {unitStatus.eva.ToString()}";
            backPage.speedText.text = $"Speed : {unitStatus.speed.ToString()}";
            InitTagText();
            backPage.unitCardIcon.color = Color.white;


        } else
        {
            frontPage.unitCardIcon.color = Color.clear;
            frontPage.unitCardIcon.sprite = null;
            frontPage.unitCardName.text = "";
        }

        if(isFront)
        {
            frontPageObject.SetActive(true);
            backPageObject.SetActive(false);
        } else
        {
            frontPageObject.SetActive(false);
            backPageObject.SetActive(true);
        }
    }

    private void InitTagText()
    {
        backPage.unitTagList = unitStatus.unitTagList;
        backPage.tagText.text = "";
        string tagText = "";

        tagText += $"<color=#00FF00>{backPage.unitNative}</color>";

        if (0 < backPage.unitTagList.Count)
        {
            tagText += ", ";
        }


        int count = 0;
        foreach (UnitTag unitTag in backPage.unitTagList)
        {
            count++;
            tagText += $"{unitTag}";

            if (count < backPage.unitTagList.Count)
            {
                tagText += $", ";
            }
        }
        tagText.Remove(tagText.Length - 2);

        backPage.tagText.text = tagText;
    }


    public void FlipCard()
    {
        if(isFront)
        {
            isFront = false;
        } else
        {
            isFront = true;
        }

        InitUnitCard();
    }
}
