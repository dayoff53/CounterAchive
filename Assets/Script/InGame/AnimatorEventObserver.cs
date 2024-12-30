using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventObserver : MonoBehaviour
{
    StageManager gameManager;

    private void Start()
    {
        gameManager = StageManager.Instance;
    }

    public void SkillHit()
    {
        if(gameManager.currentSkillData.isRandomHitProduction)
        {
            gameManager.SkillProduction(Random.Range(0, gameManager.currentSkillData.skillHitProductionObjects.Count));
        }
        else
        {
            gameManager.SkillProduction(0);
        }
        Debug.Log($"SkillHit 작동");
    }

    public void SkillEnd()
    {
        gameManager.SkillEndPlay();
        StartCoroutine(DelayTurnEnd(0.5f));
        Debug.Log($"SkillEnd 작동");
    }


    public IEnumerator DelayTurnEnd(float delay)
    {
        yield return new WaitForSeconds(delay); // 지정된 시간만큼 대기
        gameManager.TurnEnd(); // 대기 후 호출할 함수
    }
}
