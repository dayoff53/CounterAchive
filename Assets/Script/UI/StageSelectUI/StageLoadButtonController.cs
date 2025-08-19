using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor.SceneManagement;


[DetailedInfoBox("Stage를 로드할 때 사용되는 버튼의 컨트롤러", "Stage를 로드할 때 사용되는 버튼의 컨트롤러로 \n스테이지의 타입과 레벨 조건에 맞는 랜덤한 하나의 Stage를 불러옵니다.")]
public class StageLoadButtonController : MonoBehaviour
{
    private DataManager dataManager;
    private SceneChangeManager sceneChangeManager;

    [SerializeField]
    private List<Image> stageImage;
    [SerializeField]
    private List<Color> stageColors;

    [SerializeField]
    private List<Sprite> stageSprite;
    
    public int stageLevel;

    [SerializeField]
    [ReadOnly]
    private StageType _stageType;

    [DetailedInfoBox("스테이지 슬롯 상황", "해당 스테이지 위험도 \n\nStart : 시작 \nSafe : 안전 \nUnstable : 불안정 \nDanger : 위험 \nFinal : 최종")]
    [ShowInInspector]
    [EnumPaging]
    public StageType stageType
    {
        get
        {
            return _stageType;
        }
        set
        {
            _stageType = value;
            stageImage[0].color = stageColors[1];
            stageImage[1].color = stageColors[0];
            stageImage[2].color = stageColors[3];
            stageImage[3].color = stageColors[0];

            gameObject.name = $"{_stageType}_Button";
            switch (value)
            {
                case StageType.Sensei:
                    stageImage[2].color = stageColors[2];
                    stageImage[3].sprite = stageSprite[0];
                    break;
                case StageType.Arona:
                    stageImage[2].color = stageColors[2];
                    stageImage[3].sprite = stageSprite[1];
                    break;

                case StageType.Event:
                    stageImage[2].color = stageColors[3];
                    stageImage[3].sprite = stageSprite[2];
                    break;
                case StageType.Shop:
                    stageImage[2].color = stageColors[3];
                    stageImage[3].sprite = stageSprite[3];
                    break;
                case StageType.Healing:
                    stageImage[2].color = stageColors[3];
                    stageImage[3].sprite = stageSprite[4];
                    break;
                case StageType.Ramen:
                    stageImage[2].color = stageColors[3];
                    stageImage[3].sprite = stageSprite[5];
                    break;

                case StageType.Random:
                    stageImage[2].color = stageColors[4];
                    stageImage[3].sprite = stageSprite[6];
                    break;
                case StageType.Trade:
                    stageImage[2].color = stageColors[4];
                    stageImage[3].sprite = stageSprite[7];
                    break;
                case StageType.Npc:
                    stageImage[2].color = stageColors[4];
                    stageImage[3].sprite = stageSprite[8];
                    break;
                case StageType.Luckey:
                    stageImage[2].color = stageColors[4];
                    stageImage[3].sprite = stageSprite[9];
                    break;

                case StageType.Enemy:
                    stageImage[2].color = stageColors[5];
                    stageImage[3].sprite = stageSprite[10];
                    break;
                case StageType.Sukeban:
                    stageImage[2].color = stageColors[5];
                    stageImage[3].sprite = stageSprite[11];
                    break;
                case StageType.Helmet:
                    stageImage[2].color = stageColors[5];
                    stageImage[3].sprite = stageSprite[12];
                    break;
                case StageType.Robo:
                    stageImage[2].color = stageColors[5];
                    stageImage[3].sprite = stageSprite[13];
                    break;
                case StageType.Kaiser:
                    stageImage[2].color = stageColors[5];
                    stageImage[3].sprite = stageSprite[14];
                    break;

                case StageType.Boss:
                    stageImage[0].color = stageColors[6];
                    stageImage[1].color = stageColors[7];
                    stageImage[2].color = stageColors[0];
                    stageImage[3].sprite = stageSprite[15];
                    break;
                case StageType.LastBoss:
                    stageImage[0].color = stageColors[6];
                    stageImage[1].color = stageColors[7];
                    stageImage[2].color = stageColors[0];
                    stageImage[3].sprite = stageSprite[16];
                    break;
            }
        }
    }

    void Start()
    {
        dataManager = DataManager.Instance;
        sceneChangeManager = SceneChangeManager.Instance;
        Init();
    }

    public void Init()
    {
        stageType = _stageType;
        GetComponent<Button>().onClick.AddListener(LoadRandomStage);
    }

    
    /// <summary>
    /// 스테이지의 타입과 레벨 조건에 맞는 랜덤한 하나의 Stage를 불러옵니다.
    /// </summary>
    public void LoadRandomStage()
    {
        List<SceneKeyData> loadStageDataList = dataManager.sceneKeyDataList;

        if(stageType != StageType.Random)
        {
            loadStageDataList = TypeStageDatasFilter(loadStageDataList, stageType);
            loadStageDataList = LevelStageDatasFilter(loadStageDataList, stageLevel);
        }
        else
        {
            loadStageDataList = dataManager.sceneKeyDataList.Where(data => 
                data.stageType != StageType.Sensei || 
                data.stageType != StageType.Arona || 
                data.stageType != StageType.LastBoss ||
                data.stageType != StageType.Boss
                ).ToList();
        }


        if (loadStageDataList.Count == 0)
        {
            Debug.Log("스테이지 데이터가 없습니다.");
            loadStageDataList = TypeStageDatasFilter(dataManager.sceneKeyDataList, StageType.Enemy);
        }

        foreach (var item in loadStageDataList)
        {
            Debug.Log(item.stageType);
        }
        Debug.Log(loadStageDataList.Count);

        SceneKeyData loadStageData = loadStageDataList[Random.Range(0, loadStageDataList.Count)];


        sceneChangeManager.SceneLoad(loadStageData);
    }
    

    private List<SceneKeyData> TypeStageDatasFilter(List<SceneKeyData> stageDatas, StageType stageType)
    {
        List<SceneKeyData> tagStageDatas = new List<SceneKeyData>();

        if(stageDatas != null)
        {
            if(stageType == StageType.Enemy)
            {
                tagStageDatas = stageDatas.Where(data => 
                data.stageType == StageType.Sukeban
                || data.stageType == StageType.Helmet
                || data.stageType == StageType.Robo
                || data.stageType == StageType.Kaiser
                ).ToList();
            }
            else
            {
                tagStageDatas = stageDatas.Where(data => data.stageType == stageType).ToList();
            }
        }
        else
        {
            tagStageDatas = dataManager.sceneKeyDataList.Where(data => data.stageType == stageType).ToList();
        }

        return tagStageDatas;
    }

    private List<SceneKeyData> LevelStageDatasFilter(List<SceneKeyData> tagStageDatas, int stageLevel)
    {
        List<SceneKeyData> levelStageDatas = new List<SceneKeyData>();    

        if(tagStageDatas != null)
        {
            levelStageDatas = tagStageDatas.Where(data => data.stageLevel == stageLevel).ToList();
        }
        else
        {
            levelStageDatas = dataManager.sceneKeyDataList.Where(data => data.stageLevel == stageLevel).ToList();
        }

        return levelStageDatas;
    }

    private void LoadStage(SceneKeyData sceneKeyData)
    {
        sceneChangeManager.SceneLoad(sceneKeyData);
    }
}

