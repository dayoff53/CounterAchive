using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventObserver : MonoBehaviour
{
    SkillManager skillManager;
    DataManager dataManager;

    private void Start()
    {
        skillManager = SkillManager.Instance;
        dataManager = DataManager.Instance;
    }

    public void SkillHit()
    {
        skillManager.SkillHitPlay();
    }

    public void SkillEnd()
    {
        dataManager.DelayTurnEnd(0.5f);
    }
}
