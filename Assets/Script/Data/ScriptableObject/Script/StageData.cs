using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StageTag
{
    battle
}


[CreateAssetMenu(fileName = "New StageData", menuName = "Datas/StageData")]

public class StageData : ScriptableObject
{
    [SerializeField]
    public SceneKeyData sceneKeyData;

    [SerializeField]
    public StageTag stageTag;
}
