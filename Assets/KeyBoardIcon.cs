using UnityEngine;

public class KeyBoardIcon : MonoBehaviour
{
    [SerializeField]
    private GameObject keyTextObject;
    [SerializeField]
    private Animator animator; // 애니메이터 참조

    [SerializeField] 
    private Vector3 position1; // 첫 번째 위치
    [SerializeField]
    private Vector3 position2; // 두 번째 위치
    
    
    void Start()
    {
        keyTextObject.transform.localPosition = position1;
    }

    void Update()
    {
        // 애니메이터의 현재 프레임에 따라 위치 조정
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float normalizedTime = stateInfo.normalizedTime % 1f;
        
        // 애니메이션의 절반 지점을 기준으로 위치 변경
        if (normalizedTime < 0.5f)
        {
            keyTextObject.transform.localPosition = position1;
        }
        else
        {
            keyTextObject.transform.localPosition = position2;
        }
    }
}
