using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEditor;
using System.IO;

/// <summary>
/// 세이브 데이터 클래스
/// </summary>
[System.Serializable]
public class SaveData
{
    public List<UnitStatus> playerUnitStates;
}
public class DataManager : Singleton<DataManager>
{
    /// <summary>
    /// SaveData가 저장될 혹은 불러올 위치
    /// </summary>
    [SerializeField]
    string saveDataFilePath;
    public SaveData currentSaveData;

    /// <summary>
    /// 플레이어가 보유한 유닛의 State List
    /// </summary>
    public List<UnitStatus> playerUnitStateList;

    /// <summary>
    /// UnitData를 보관할 때 스프라이트, 이미지와 애니메이션 등의 리소스를 주로 불러와 보관한다.
    /// </summary>
    public List<UnitData> unitDataList;

    /// <summary>
    /// SkillData를 보관하는 List
    /// </summary>
    public List<SkillData> skillList;


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

        
            //currentSaveData = new SaveData();
            Debug.Log("게임 테스트중이기 때문에 세이브 데이터를 불러옵니다.");
            LoadGame();
        
    }

    /// <summary>
    /// 현 게임 데이터를 저장합니다.
    /// </summary>
    /// <returns></returns>
    public void SaveGame()
    {
        currentSaveData.playerUnitStates = playerUnitStateList;

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