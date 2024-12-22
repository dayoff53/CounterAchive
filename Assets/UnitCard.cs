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
        [Header("카드 앞면")]
        [Tooltip("유닛의 얼굴 아이콘")]
        public Image unitCardIcon;

        [Tooltip("유닛의 이름 텍스트")]
        public TMP_Text unitCardName;

        [Tooltip("유닛의 Hp 게이지")]
        public Image currentTurnHpGaugeBar;

        [Tooltip("유닛의 Hp 텍스트")]
        public TMP_Text currentTurnHpText;
    }

    [System.Serializable]
    struct BackPage
    {
        [Header("카드 앞면")]
        [Tooltip("유닛의 얼굴 아이콘")]
        public Image unitCardIcon;

        [Tooltip("유닛의 이름 텍스트")]
        public TMP_Text unitCardName;

        [Tooltip("유닛의 Hp 게이지")]
        public Image hpGaugeBar;

        [Tooltip("유닛의 Hp 텍스트")]
        public TMP_Text hpText;
        [Tooltip("유닛의 Atk 텍스트")]
        public TMP_Text atkText;
        [Tooltip("유닛의 Def 텍스트")]
        public TMP_Text defText;
        [Tooltip("유닛의 Acc 텍스트")]
        public TMP_Text accText;
        [Tooltip("유닛의 Eva 텍스트")]
        public TMP_Text evaText;
        [Tooltip("유닛의 Speed 텍스트")]
        public TMP_Text speedText;

        [Tooltip("유닛의 Tag 텍스트")]
        public TMP_Text tagText;
        public UnitNative unitNative;
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
