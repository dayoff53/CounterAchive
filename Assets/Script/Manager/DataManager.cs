using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEditor;
using Sirenix.OdinInspector;
using System.IO;


/// <summary>
/// 세이브 데이터 클래스
/// </summary>
[System.Serializable]
public class SaveData
{
    public List<UnitStatus> playerUnitStates;

/// <summary>
/// 스테이지의 마지막 넘버
/// </summary>
    public int lastStageNumber;

/// <summary>
/// 스테이지의 현재 넘버
/// </summary>
    public int currentStageNumber;

/// <summary>
/// 게임에서 플레이했던 과거 스테이지의 리스트
/// </summary>
    public List<SceneKeyData> playedSceneKeyDataList;
}
public class DataManager : Singleton<DataManager>
{
    public BattleStageMaster battleStageMaster;


    /// <summary>
    /// SaveData가 저장될 혹은 불러올 위치
    /// </summary>
    [SerializeField]
    [InfoBox("SaveData가 저장될 혹은 불러올 위치")]
    string saveDataFilePath;
    public SaveData currentSaveData;

    /// <summary>
    /// 플레이어가 보유한 유닛의 State List
    /// </summary>
    public List<UnitStatus> playerUnitStateList;
    
/// <summary>
/// 스테이지의 마지막 넘버
/// </summary>
    public int lastStageNumber;

/// <summary>
/// 스테이지의 현재 넘버
/// </summary>
    public int currentStageNumber;

    /// <summary>
    /// UnitData를 보관할 때 스프라이트, 이미지와 애니메이션 등의 리소스를 주로 불러와 보관한다.
    /// </summary>
    public List<UnitData> unitDataList;

    /// <summary>
    /// SkillData를 보관하는 List
    /// </summary>
    public List<SkillData> skillList;

    /// <summary>
    /// 스테이지 데이터를 보관하는 List
    /// </summary>
    public List<SceneKeyData> sceneKeyDataList;

/// <summary>
/// 유닛 상태에 따른 색과 레이어 순서를 보관하는 스크립트
/// </summary>
    public ColorState unitColorStateObject;



    private void Start()
    {
        DataInit();
    }

    private void DataInit()
    {
        unitDataList = new List<UnitData>(Resources.LoadAll<UnitData>("ScriptableObject/UnitData"));
        unitDataList.Sort((unit1, unit2) => unit1.unitNumber.CompareTo(unit2.unitNumber));

        skillList = new List<SkillData>(Resources.LoadAll<SkillData>("ScriptableObject/SkillData"));
        saveDataFilePath = Path.Combine(Application.persistentDataPath, "saveData.json");

        sceneKeyDataList = new List<SceneKeyData>(Resources.LoadAll<SceneKeyData>("ScriptableObject/SceneKeyData"));
        unitColorStateObject = Resources.Load<ColorState>("ScriptableObject/ColorState");

        if(Application.isEditor)
        {
            //currentSaveData = new SaveData();
            Debug.Log("게임 테스트중이기 때문에 세이브 데이터를 불러최초 게임 진행 시 세이브 데이터를 불러옵니다.`");
            LoadGame();
        }
        else
        {
            Debug.Log("게임 테스트중이 아니기 때문에 세이브 데이터를 불러오지 않습니다.");
        }

        

    }

    /// <summary>
    /// 현 게임 데이터를 저장합니다.
    /// </summary>
    /// <returns></returns>
    public void SaveGame()
    {
        currentSaveData.playerUnitStates = playerUnitStateList;
        currentSaveData.lastStageNumber = lastStageNumber;
        currentSaveData.currentStageNumber = currentStageNumber;

        string json = JsonUtility.ToJson(currentSaveData, true);

        File.WriteAllText(saveDataFilePath, json);

        Debug.Log("Game Save Complete!!");
    }

    /// <summary>
    /// 게임 데이터를 불러와 반환합니다.
    /// </summary>
    /// <returns></returns>
    public SaveData LoadData()
    {
        if (File.Exists(saveDataFilePath))
        {
            string json = File.ReadAllText(saveDataFilePath);

            SaveData data = JsonUtility.FromJson<SaveData>(json);

            Debug.Log("Game Load Complete!!");

            return data;
        }
        else
        {
            Debug.Log("Game Load Fail...");
            return null;
        }
    }

    public void LoadGame()
    {
        currentSaveData = LoadData();


        if (currentSaveData == null)
        {
            currentSaveData = new SaveData();

            Debug.Log("SaveData is exist");
        }
        else
        {
            playerUnitStateList = new List<UnitStatus>();

            foreach (UnitStatus unitState in currentSaveData.playerUnitStates)
            {
                if(unitState.unitData == null)
                {
                    unitState.unitData = unitDataList.Find(un => un.unitNumber == unitState.unitNumber);
                }
                playerUnitStateList.Add(unitState);
            }

            lastStageNumber = currentSaveData.lastStageNumber;
            currentStageNumber = currentSaveData.currentStageNumber;

            Debug.Log("SaveData do not exist");
        }
    }

    /// <summary>
    /// Resources 폴더에서 특정 리소스를 찾아오는 스크립트
    /// </summary>
    /// <param name="resource"></param>
    /// <returns></returns>
    private string GetResourcePath(Object resource)
    {
        if (resource == null)
        {
            return string.Empty;
        }

        string path = AssetDatabase.GetAssetPath(resource);

        int resourcesIndex = path.IndexOf("Resources/");
        if (resourcesIndex >= 0)
        {
            path = path.Substring(resourcesIndex + "Resources/".Length); // 'Resources/' 주소 제거
            path = path.Replace(System.IO.Path.GetExtension(path), ""); // 확장자 제거
        }

        return path;
    }
    
}