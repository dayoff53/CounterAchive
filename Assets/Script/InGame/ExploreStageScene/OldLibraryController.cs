using UnityEngine;
using UnityEngine.Events;

public class InAndOutTriggerController : MonoBehaviour
{
    [SerializeField]
    private GameObject[] outLibrary;
    [SerializeField]
    private Animator doorAnim;
    [SerializeField] 
    private float fadeSpeed = 2f;
    private SpriteRenderer[] spriteRenderers;
    private float[] targetAlpha;

    [SerializeField]
    private UnityEvent outEvent;
    [SerializeField]
    private UnityEvent inEvent;

    void Start()
    {
        // SpriteRenderer 컴포넌트들을 캐싱
        spriteRenderers = new SpriteRenderer[outLibrary.Length];
        targetAlpha = new float[outLibrary.Length];
        
        for (int i = 0; i < outLibrary.Length; i++)
        {
            spriteRenderers[i] = outLibrary[i].GetComponent<SpriteRenderer>();
            targetAlpha[i] = spriteRenderers[i].color.a;
        }
    }

    void Update()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            Color currentColor = spriteRenderers[i].color;
            float newAlpha = Mathf.Lerp(currentColor.a, targetAlpha[i], Time.deltaTime * fadeSpeed);
            spriteRenderers[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            Debug.Log("OnTriggerEnter2D \nCollider2D : " + other.gameObject.name);
            for (int i = 0; i < outLibrary.Length; i++)
            {
                targetAlpha[i] = 1f;
                outLibrary[i].SetActive(true);
            }
            doorAnim.SetTrigger("Close");
            outEvent.Invoke();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            Debug.Log("OnTriggerExit2D \nCollider2D : " + other.gameObject.name);
            for (int i = 0; i < outLibrary.Length; i++)
            {
                targetAlpha[i] = 0f;
            }
            doorAnim.SetTrigger("Open");
            inEvent.Invoke();
        }
    }
}
