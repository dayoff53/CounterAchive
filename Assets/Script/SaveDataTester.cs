using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveDataTester : MonoBehaviour
{
    [SerializeField]
    private List<SkillData> skillDatas;

    public void Start()
    {
        for(int i = 0; skillDatas.Count > i; i++)
        {
            SaveSkillDataToJson(skillDatas[i]);
        }
    }

    public void SaveSkillDataToJson(SkillData skillData)
    {
        string jsonData = JsonUtility.ToJson(skillData);
        File.WriteAllText(Application.persistentDataPath + "/skillData.json", jsonData);
    }
}
