using UnityEngine;
using Sirenix.OdinInspector;

[InfoBox("Scene 변경 시 DataManager")]
public class DataManagerLinker : MonoBehaviour
{
    [ReadOnly]    
    private DataManager dataManager;

    [SerializeField] private BattleStageMaster battleStageMaster;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dataManager = DataManager.Instance;
        
        if(battleStageMaster != null)
        {
            dataManager.battleStageMaster = battleStageMaster;
        }
    }

}
