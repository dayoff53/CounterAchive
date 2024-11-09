using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// �پ��� ���������� �ҷ��� �� �ִ� �޴���. Ư�� ���ǿ� �����ϴ� ���������� ���� Ȥ�� �������� �ҷ��� �� �ִ�.
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
    /// ������ ���� Stage�� �ҷ��ɴϴ�.
    /// </summary>
    public void LoadRandomStage()
    {
        int randomIndex = Random.Range(0, stageDatas.Count);

        StageData loadStageData = stageDatas[Random.Range(0, stageDatas.Count)];

        sceneChangeManager.SceneLoad(loadStageData.sceneKeyData);
    }

    /// <summary>
    /// ������ ���� Stage�� �ҷ��ɴϴ�.
    /// </summary>
    /// <param name="stageTag">Ư�� Tag�� Stage���� �ҷ��� �� �ֽ��ϴ�.</param>
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
