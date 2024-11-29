using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEditor;
using System.IO;

/// <summary>
/// ���̺� ������ Ŭ����
/// </summary>
[System.Serializable]
public class SaveData
{
    public List<UnitStatus> playerUnitStates;
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
    public List<UnitStatus> playerUnitStateList;

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
            playerUnitStateList = new List<UnitStatus>();

            foreach (UnitStatus unitState in currentSaveData.playerUnitStates)
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