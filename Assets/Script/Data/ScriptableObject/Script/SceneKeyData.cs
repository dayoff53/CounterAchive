using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StageType
{
    Sensei,
    Arona,
    Event,
    Shop,
    Healing,
    Ramen,
    Random,
    Trade,
    Npc,
    Luckey,
    Enemy,
    Sukeban,
    Helmet,
    Robo,
    Kaiser,
    Boss,
    LastBoss
}

[CreateAssetMenu(fileName = "SceneKeyData", menuName = "Datas/SceneKeyData", order = 0)]
public class SceneKeyData : ScriptableObject
{
    public string sceneName;

    [SerializeField]
    public StageType stageType;

    [SerializeField]
    public int stageLevel;
}
