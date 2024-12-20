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
        StartCoroutine(DelayTurnEnd(0.5f));
        Debug.Log($"SkillEnd �۵�");
    }


    public IEnumerator DelayTurnEnd(float delay)
    {
        yield return new WaitForSeconds(delay); // ������ �ð���ŭ ���
        gameManager.TurnEnd(); // ��� �� ȣ���� �Լ�
    }
}
