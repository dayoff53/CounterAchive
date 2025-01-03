using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StageTag
{
    battle
}


[CreateAssetMenu(fileName = "New StageData", menuName = "Datas/StageData")]

public class StageLoadData : SceneKeyData
{
    [SerializeField]
    public SceneKeyData sceneKeyData;

    [SerializeField]
    public StageTag stageTag;
}
