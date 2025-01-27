using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventObserver : MonoBehaviour
{
    [SerializeField]
    private StageMaster stageManager;

    public void Init(StageMaster stageManager)
    {
        this.stageManager = stageManager;
    }

    public void SkillHit()
    {
        if(stageManager.currentSkillData.isRandomHitProduction)
        {
            stageManager.SkillProduction(Random.Range(0, stageManager.currentSkillData.skillHitProductionObjects.Count));
        }
        else
        {
            stageManager.SkillProduction(0);
        }
        Debug.Log($"SkillHit 작동");
    }

    public void SkillEnd()
    {
        stageManager.SkillEndPlay();
        stageManager.TurnEnd();
        Debug.Log($"SkillEnd 작동");
    }


    public IEnumerator DelayTurnEnd(float delay)
    {
        yield return new WaitForSeconds(delay); // 지정된 시간만큼 대기
        stageManager.TurnEnd(); // 대기 후 호출할 함수
    }
}
