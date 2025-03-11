using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

[DetailedInfoBox("탐험 스테이지에서 상호작용 가능한 오브젝트를 제어하는 컨트롤러", "탐험 스테이지에서 상호작용 가능한 오브젝트를 제어하는 컨트롤러로 \nExploreStageMaster의 currentInteract에 할당되어 사용됩니다.")]
public class InteractObjectController : MonoBehaviour
    {
        [SerializeField]
        [InfoBox("탐험 스테이지를 관리하는 매니저")]
        private ExploreStageMaster exploreStageMaster;

        [SerializeField]
        [InfoBox("상호작용 시 발생하는 이벤트")]
        public UnityEvent InteractEvent;
        
        [SerializeField]
        [InfoBox("상호작용 가능 대상이 되었을 경우 발생하는 이벤트")]
        public UnityEvent InInteractEvent;

        [SerializeField]
        [InfoBox("상호작용 가능 대상에서 제외되었을 경우 발생하는 이벤트")]
        public UnityEvent outInteractEvent;

        void Reset()
        {
            exploreStageMaster = GameObject.Find("ExploreStageMaster").GetComponent<ExploreStageMaster>();
        }



        void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                exploreStageMaster.SetCurrentInteract(this);
            }
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {   
                if (exploreStageMaster.currentInteract == this)
                {   
                    exploreStageMaster.SetCurrentInteract(null);
                }
            }
        }
    }
