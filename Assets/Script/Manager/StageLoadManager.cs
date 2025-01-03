using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 다양한 스테이지를 불러올 수 있는 매니저. 특정 조건에 부합하는 스테이지만 랜덤 혹은 순차적으로 불러올 수 있다.
/// </summary>
public class StageLoadManager : Singleton<StageLoadManager>
{
    [SerializeField]
    private List<StageLoadData> stageDatas;
    [SerializeField]
    private SceneChangeManager sceneChangeManager;

    private void Start()
    {
        sceneChangeManager = SceneChangeManager.Instance;
    }

    /// <summary>
    /// 랜덤한 하나의 Stage를 불러옵니다.
    /// </summary>
    public void LoadRandomStage()
    {
        int randomIndex = Random.Range(0, stageDatas.Count);

        StageLoadData loadStageData = stageDatas[Random.Range(0, stageDatas.Count)];

        sceneChangeManager.SceneLoad(loadStageData.sceneKeyData);
    }

    /// <summary>
    /// 랜덤한 하나의 Stage를 불러옵니다.
    /// </summary>
    /// <param name="stageTag">특정 Tag의 Stage만을 불러올 수 있습니다.</param>
    public void LoadRandomStage(StageTag stageTag)
    {
        int randomIndex = Random.Range(0, stageDatas.Count);

        List<StageLoadData> tagStageDatas = new List<StageLoadData>();

        for (int i = 0; i <= stageDatas.Count; i++)
        {
            if(stageDatas[i].stageTag == stageTag)
            {
                tagStageDatas.Add(stageDatas[i]);
            }
        }

        StageLoadData loadStageData = tagStageDatas[Random.Range(0, tagStageDatas.Count)];

        sceneChangeManager.SceneLoad(loadStageData.sceneKeyData);
    }
}
