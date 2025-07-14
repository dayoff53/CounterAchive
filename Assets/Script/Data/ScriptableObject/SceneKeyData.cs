using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

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

public enum StageClearState
{
    KillAllEnemy,
    KillTargetEnemy,
    SurviveTurn
}

public enum ProgressState
{
    Stay,
    UnitPlay,

    SkillTargetSearch,
    SkillPlay,

    UnitSelect,
    GameEnd
}

[CreateAssetMenu(fileName = "SceneKeyData", menuName = "Datas/SceneKeyData", order = 0)]
public class SceneKeyData : ScriptableObject
{
    [DetailedInfoBox("스테이지 이름", "00의 자리 숫자: 현재 스테이지가 속한 레벨(Level) 을 의미합니다. \n나머지 숫자 (두 자리 이하): 해당 레벨 내에서의 스테이지 번호(Stage Number) 를 나타냅니다. \n\n\n 예시:\nHelmet_312 => 레벨 3의 스테이지 12번")]
    public string sceneName;

    [InfoBox("스테이지 번호 (stageName이 'Helmet_104' 일 경우 해당 스테이지의 번호는 4)")]
    public int stageNumber;

    [SerializeField]
    [InfoBox("스테이지 레벨(난이도)")]
    public int stageLevel;

    [SerializeField]
    [InfoBox("스테이지 타입 (예시 : 헬멧단 전투, 상점, 휴게소 등)")]
    public StageType stageType;

    [InfoBox("스테이지 클리어 조건")]
    public StageClearState stageClearCondition;

    [InfoBox("KillTargetEnemy 조건일 경우 특정 적의 ID")]
    public int targetEnemyId;

    [InfoBox("최근 사망한 마지막 적")]
    public GameObject lastEnemyDeathObject;

    [InfoBox("SurviveTurn 조건일 경우 생존해야 할 턴 수")]
    public int surviveTurnCount;

    [InfoBox("해당 스테이지에서 플레이어가 사용 가능하도록 사전에 배치되어 있는 유닛 리스트")]
    public List<UnitStatus> playerUnitList;

    [InfoBox("해당 스테이지에서 적으로 등장하는 유닛 리스트")]
    public List<UnitStatus> enemyUnitList;
}
