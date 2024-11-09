using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionController : MonoBehaviour
{
    private DataManager dataManager;

    [SerializeField]
    private GameObject optionObject;
    private bool isGamePaused = false;

    void Start()
    {
        dataManager = DataManager.Instance;

        if (optionObject != null)
        {
            optionObject.SetActive(false); // ���� ���� �� �Ͻ����� �г� ��Ȱ��ȭ
        }
    }

    // �� �����Ӹ��� ȣ��Ǵ� ������Ʈ �޼���
    void Update()
    {
        // ESC Ű�� ���� ������ ���߰ų� �簳
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f; // ���� �ð� ����
        isGamePaused = true;
        if (optionObject != null)
        {
            optionObject.SetActive(true); // �Ͻ����� UI Ȱ��ȭ
        }
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f; // ���� �ð� �簳
        isGamePaused = false;
        if (optionObject != null)
        {
            optionObject.SetActive(false); // �Ͻ����� UI ��Ȱ��ȭ
        }
    }


    public void GameSave()
    {
        dataManager.SaveGame();
    }
}
