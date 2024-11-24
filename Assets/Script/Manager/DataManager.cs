using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 세이브 데이터 클래스
/// </summary>
[System.Serializable]
public class SaveData
{
    public List<UnitState> playerUnitStates;
}

/// <summary>
/// 유닛의 현 상태에 관한 데이터 (Json에 저장될 데이터)
/// </summary>
[System.Serializable]
public class UnitState
{
    [Header("Unit States")]
    public UnitData defaultUnitData;
    public int unitNumber = 0;
    public string unitName = "Null";
    public float hp = 0;
    public int atk = 0;
    public int speed = 1;
    public float actionPoint = 0;
    public List<int> skillNumberList; // skillDataList 대신 스킬의 식별자(Number)를 사용하여 저장
    
    public UnitState()
    {
        ApplyBaseState(defaultUnitData);
    }

    public UnitState(UnitData unitData)
    {
        unitNumber = unitData.unitNumber;

            ApplyBaseState(unitData);
    }

    /// <summary>
    /// 기본 유닛 데이터를 적용
    /// </summary>
    /// <param name="unitData">기준이 될 UnitData</param>
    public void ApplyBaseState(UnitData unitData)
    {
        if (unitData == null) return;

        unitNumber = unitData.unitNumber;
        unitName = unitData.unitName;
        hp = unitData.hp;
        atk = unitData.atk;
        speed = unitData.speed;
        actionPoint = unitData.actionPoint;

        // skillDataList에 있는 각 SkillData의 ID만 저장
        skillNumberList = new List<int>();
        foreach (SkillData skill in unitData.skillDataList)
        {
            skillNumberList.Add(skill.skillNumber);
        }
    }

    /// <summary>
    /// 추가 스텟를 적용 (기본 스텟에 더해주는 방식)
    /// </summary>
    /// <param name="unitData">기준이 될 UnitData</param>
    public void ApplyPlusState(UnitData unitData)
    {
        if (unitData == null) return;

        unitNumber = unitData.unitNumber;
        unitName = unitData.unitName;

        // 기존 값에 unitData의 값을 더해줌
        hp += unitData.hp;
        atk += unitData.atk;
        speed += unitData.speed;
        actionPoint += unitData.actionPoint;

        // skillDataList에 있는 각 SkillData의 Number(ID값)만 추가 (중복되지 않도록 관리)
        if (skillNumberList == null)
        {
            skillNumberList = new List<int>();
        }

        foreach (SkillData skill in unitData.skillDataList)
        {
            if (!skillNumberList.Contains(skill.skillNumber))
            {
                skillNumberList.Add(skill.skillNumber);
            }
        }
    }

}

public class DataManager : Singleton<DataManager>
{
    /// <summary>
    /// SaveData가 저장된 혹은 저장될 위치
    /// </summary>
    [SerializeField]
    string saveDataFilePath;

    public SaveData currentSaveData;

    /// <summary>
    /// 플레이어가 사용할 유닛의 State List
    /// </summary>
    public List<UnitState> playerUnitStateList;

    /// <summary>
    /// UnitData를 저장해 둔 저장소, 이미지나 애니메이션 등의 리소스를 주로 불러 사용한다.
    /// </summary>
    public List<UnitData> unitDataList;
    /// <summary>
    /// SkillData를 저장해둔 List
    /// </summary>
    public List<SkillData> skillList;


    /// <summary>
    /// 게임 실행시 세이브 데이터의 여부를 판단 후 만약 세이브 데이터가 존재한다면 데이터를 가져옵니다.
    /// </summary>
    private void Start()
    {
        DataInit();
    }

    private void DataInit()
    {
        unitDataList = new List<UnitData>(Resources.LoadAll<UnitData>("ScriptableObject/UnitData"));
        skillList = new List<SkillData>(Resources.LoadAll<SkillData>("ScriptableObject/SkillData"));
        saveDataFilePath = Path.Combine(Application.persistentDataPath, "saveData.json");

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
    /// 저장 데이터를 불러와 출력합니다.
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
            playerUnitStateList = new List<UnitState>();

            foreach (UnitState unitState in currentSaveData.playerUnitStates)
            {
                if(unitState.defaultUnitData == null)
                {
                    unitState.defaultUnitData = unitDataList.Find(un => un.unitNumber == unitState.unitNumber);
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