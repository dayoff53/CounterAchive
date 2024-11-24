using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// ���̺� ������ Ŭ����
/// </summary>
[System.Serializable]
public class SaveData
{
    public List<UnitState> playerUnitStates;
}

/// <summary>
/// ������ �� ���¿� ���� ������ (Json�� ����� ������)
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
    public List<int> skillNumberList; // skillDataList ��� ��ų�� �ĺ���(Number)�� ����Ͽ� ����
    
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
    /// �⺻ ���� �����͸� ����
    /// </summary>
    /// <param name="unitData">������ �� UnitData</param>
    public void ApplyBaseState(UnitData unitData)
    {
        if (unitData == null) return;

        unitNumber = unitData.unitNumber;
        unitName = unitData.unitName;
        hp = unitData.hp;
        atk = unitData.atk;
        speed = unitData.speed;
        actionPoint = unitData.actionPoint;

        // skillDataList�� �ִ� �� SkillData�� ID�� ����
        skillNumberList = new List<int>();
        foreach (SkillData skill in unitData.skillDataList)
        {
            skillNumberList.Add(skill.skillNumber);
        }
    }

    /// <summary>
    /// �߰� ���ݸ� ���� (�⺻ ���ݿ� �����ִ� ���)
    /// </summary>
    /// <param name="unitData">������ �� UnitData</param>
    public void ApplyPlusState(UnitData unitData)
    {
        if (unitData == null) return;

        unitNumber = unitData.unitNumber;
        unitName = unitData.unitName;

        // ���� ���� unitData�� ���� ������
        hp += unitData.hp;
        atk += unitData.atk;
        speed += unitData.speed;
        actionPoint += unitData.actionPoint;

        // skillDataList�� �ִ� �� SkillData�� Number(ID��)�� �߰� (�ߺ����� �ʵ��� ����)
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
    /// SaveData�� ����� Ȥ�� ����� ��ġ
    /// </summary>
    [SerializeField]
    string saveDataFilePath;

    public SaveData currentSaveData;

    /// <summary>
    /// �÷��̾ ����� ������ State List
    /// </summary>
    public List<UnitState> playerUnitStateList;

    /// <summary>
    /// UnitData�� ������ �� �����, �̹����� �ִϸ��̼� ���� ���ҽ��� �ַ� �ҷ� ����Ѵ�.
    /// </summary>
    public List<UnitData> unitDataList;
    /// <summary>
    /// SkillData�� �����ص� List
    /// </summary>
    public List<SkillData> skillList;


    /// <summary>
    /// ���� ����� ���̺� �������� ���θ� �Ǵ� �� ���� ���̺� �����Ͱ� �����Ѵٸ� �����͸� �����ɴϴ�.
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
    /// �� ���� �����͸� �����մϴ�.
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
    /// ���� �����͸� �ҷ��� ����մϴ�.
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
    /// Resources �������� Ư�� ���ҽ��� ã�ƿ��� ��ũ��Ʈ
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
            path = path.Substring(resourcesIndex + "Resources/".Length); // 'Resources/' �ּ� ����
            path = path.Replace(System.IO.Path.GetExtension(path), ""); // Ȯ���� ����
        }

        return path;
    }
}