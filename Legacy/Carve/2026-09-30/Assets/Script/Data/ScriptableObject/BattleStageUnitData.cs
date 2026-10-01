using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.IO;


[CreateAssetMenu(fileName = "BattleStageUnitData", menuName = "Scriptable Objects/BattleStageUnitData")]
[DetailedInfoBox("전투 스테이지 데이터", "BattleStageMaster가 전투 스테이지를 구성할 때 필요한 데이터를 저장하는 스크립트")]
public class BattleStageUnitData : ScriptableObject
{
    [InfoBox("스테이지 번호")]
    public int stageNumber;

    [InfoBox("스테이지 레벨(난이도)")]
    public int stageLevel;

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


/*
    [Button("스테이지 데이터 저장")]
    public void SaveStageData()
    {
        string fileName = $"{stageType}_{stageLevel}{stageNumber:D2}.json";
        string filePath = Path.Combine(Application.dataPath, "StageData", fileName);

        // 디렉토리가 없으면 생성
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        // 저장할 데이터 생성
        var saveData = new
        {
            stageNumber = this.stageNumber,
            stageLevel = this.stageLevel,
            stageType = this.stageType.ToString(),
            stageClearCondition = this.stageClearCondition.ToString(),
            targetEnemyId = this.targetEnemyId,
            surviveTurnCount = this.surviveTurnCount,
            playerUnitList = this.playerUnitList,
            enemyUnitList = this.enemyUnitList
        };

        // JSON으로 변환하여 저장
        string jsonData = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(filePath, jsonData);

        Debug.Log($"스테이지 데이터가 저장되었습니다: {filePath}");
    }
*/