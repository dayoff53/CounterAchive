using System.Collections.Generic;
using UnityEngine;

public class StageLoadButtonListController : MonoBehaviour
{
    public List<GameObject> StageButtonPrefab;

    void Start()
    {
        Init();
    }

    public void Init()
    {
        foreach (var stageButton in StageButtonPrefab)
        {
            StageLoadButtonController stageSelectController = stageButton.GetComponent<StageLoadButtonController>();
            stageSelectController.stageType = (StageType)Random.Range(2, System.Enum.GetValues(typeof(StageType)).Length - 2);
            stageSelectController.Init();
        }
    }
}
