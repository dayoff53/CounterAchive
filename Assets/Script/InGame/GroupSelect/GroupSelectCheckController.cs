using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// ���� ���� �ʱ� �÷����� �׷��� ���� �������� �Ѿ�� �ӽ� ��ũ��Ʈ
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
