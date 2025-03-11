using UnityEngine;
using Sirenix.OdinInspector;

[DetailedInfoBox("탐험 스테이지를 관리하는 매니저", "탐험 스테이지에서 플레이어와 상호작용 가능한 오브젝트를 제어하는 매니저로 \n해당 스크립트가 존재하는 스테이지의 기본 정보를 가지고 있으며, 상호작용 등의 전반적인 탐험 기능을 총괄합니다.")]
public class ExploreStageMaster : MonoBehaviour
{
    [SerializeField]
    private ExplorePlayerController explorePlayerController;

    [SerializeField]
    public InteractObjectController currentInteract;

    public void SetCurrentInteract(InteractObjectController interactObjectController)
    {
        if (currentInteract != null)
        {
            currentInteract.outInteractEvent.Invoke();
        }
        currentInteract = interactObjectController;
        if (currentInteract != null)
        {
            currentInteract.gameObject.SetActive(true);
            currentInteract.InInteractEvent.Invoke();
        }
    }

    public void BattleStageSelect()
    {
        if (currentInteract != null)
        {
            currentInteract.gameObject.SetActive(true);
        }
    }
}
