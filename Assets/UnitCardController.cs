using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitCardController : MonoBehaviour
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
        [SerializeField]
        [Tooltip("유닛의 얼굴 아이콘")]
        public Image unitCardIcon;

        [SerializeField]
        [Tooltip("유닛의 이름 텍스트")]
        public TMP_Text unitCardName;

        [Tooltip("유닛의 Hp 게이지")]
        [SerializeField]
        public Image currentTurnHpGaugeBar;

        [SerializeField]
        [Tooltip("유닛의 Hp 텍스트")]
        public TMP_Text currentTurnHpText;
    }

    [System.Serializable]
    struct BackPage
    {
        [Header("카드 앞면")]
        [SerializeField]
        [Tooltip("유닛의 얼굴 아이콘")]
        private Image unitCardIcon;

        [SerializeField]
        [Tooltip("유닛의 이름 텍스트")]
        private TMP_Text unitCardName;
    }

    [SerializeField]
    private FrontPage frontPage;
    [SerializeField]
    private BackPage backPage;
    public Image cardImage;


    // Start is called before the first frame update
    void Start()
    {
        dataManager = DataManager.Instance;
        cardImage = GetComponent<Image>();
        InitUnitCard();
    }

    public void InitUnitCard()
    {
        if (unitStatus.unitNumber != 0)
        {
            frontPage.unitCardIcon.sprite = dataManager.unitDataList.Find(u => u.unitNumber == unitStatus.unitNumber).unitFaceIcon;
            frontPage.unitCardName.text = unitStatus.unitName;
        }
    }
}
