using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 게임 시작 초기 플레이할 그룹을 고르고 스테이지 넘어가는 임시 스크립트
/// </summary>
public class GroupSelectCheckController : MonoBehaviour
{
    public Button groupSelectCheckButton;



    public void GameStart()
    {
        StageLoadManager.Instance.LoadRandomStage();
    }
    public void SelectCancel()
    {
        gameObject.SetActive(false);
    }
}
