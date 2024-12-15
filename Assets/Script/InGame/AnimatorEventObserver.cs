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
        gameManager.SkillProduction(0);
        Debug.Log($"SkillHit �۵�");
    }

    public void SkillEnd()
    {
        gameManager.SkillEndPlay();
        gameManager.DelayTurnEnd(0.5f);
        Debug.Log($"SkillEnd �۵�");
    }
}
