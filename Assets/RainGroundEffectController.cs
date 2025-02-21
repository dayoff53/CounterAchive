using UnityEngine;

public class RainGroundEffectController : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private Animator animator;
    private bool isAnimationFinished = false;

    void Start()
    {
        spriteRenderer.color = new Color(1, 1, 1, Random.Range(0.1f, 0.5f));
    }


    void Update()
    {
        if (!isAnimationFinished && animator != null)
        {
            // 현재 애니메이터의 상태 정보 가져오기
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // 현재 애니메이션이 끝났는지 확인
            if (stateInfo.normalizedTime >= 1.0f)
            {
                isAnimationFinished = true;
                Destroy(gameObject);
            }
        }
    }
}
