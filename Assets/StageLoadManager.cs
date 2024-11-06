using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 다양한 스테이지를 불러올 수 있는 메니저. 특정 조건에 충족하는 스테이지를 지정 혹은 렌덤으로 불러올 수 있다.
/// </summary>
public class StageLoadManager : Singleton<StageLoadManager>
{
    [SerializeField]
    private List<StageData> stageDatas;
    [SerializeField]
    private SceneChangeManager sceneChangeManager;

    private void Start()
    {
        sceneChangeManager = SceneChangeManager.Instance;
    }

    /// <summary>
    /// 렌덤한 값의 Stage를 불러옵니다.
    /// </summary>
    public void LoadRandomStage()
    {
        int randomIndex = Random.Range(0, stageDatas.Count);

        StageData loadStageData = stageDatas[Random.Range(0, stageDatas.Count)];

        sceneChangeManager.SceneLoad(loadStageData.sceneKeyData);
    }

    /// <summary>
    /// 렌덤한 값의 Stage를 불러옵니다.
    /// </summary>
    /// <param name="stageTag">특정 Tag의 Stage만을 불러올 수 있습니다.</param>
    public void LoadRandomStage(StageTag stageTag)
    {
        int randomIndex = Random.Range(0, stageDatas.Count);

        List<StageData> tagStageDatas = new List<StageData>();

        for (int i = 0; i <= stageDatas.Count; i++)
        {
            if(stageDatas[i].stageTag == stageTag)
            {
                tagStageDatas.Add(stageDatas[i]);
            }
        }

        StageData loadStageData = tagStageDatas[Random.Range(0, tagStageDatas.Count)];

        sceneChangeManager.SceneLoad(loadStageData.sceneKeyData);
    }
}
