    using UnityEngine;

    public class UiTableController : MonoBehaviour
    {
        [SerializeField]
        private GameObject keyBoardIcon;

        void Start()
        {
            keyBoardIcon.SetActive(false);
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                keyBoardIcon.SetActive(true);
            }
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {   
                keyBoardIcon.SetActive(false);
            }
        }
    }
