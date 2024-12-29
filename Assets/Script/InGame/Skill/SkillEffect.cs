using UnityEngine;

public class SkillEffect : MonoBehaviour
{
    
[System.Serializable]
[Tooltip("스킬 효과를 정의하는 클래스")]
public class SkillEffect
{
    [Tooltip("스킬 효과의 상태")]
    public SkillEffectState skillEffectState;

    [Tooltip("스킬 효과의 값 리스트")]
    public List<float> valueList;
}

}
