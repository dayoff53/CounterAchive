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
        [Header("ī�� �ո�")]
        [SerializeField]
        [Tooltip("������ �� ������")]
        public Image unitCardIcon;

        [SerializeField]
        [Tooltip("������ �̸� �ؽ�Ʈ")]
        public TMP_Text unitCardName;

        [Tooltip("������ Hp ������")]
        [SerializeField]
        public Image currentTurnHpGaugeBar;

        [SerializeField]
        [Tooltip("������ Hp �ؽ�Ʈ")]
        public TMP_Text currentTurnHpText;
    }

    [System.Serializable]
    struct BackPage
    {
        [Header("ī�� �ո�")]
        [SerializeField]
        [Tooltip("������ �� ������")]
        private Image unitCardIcon;

        [SerializeField]
        [Tooltip("������ �̸� �ؽ�Ʈ")]
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
