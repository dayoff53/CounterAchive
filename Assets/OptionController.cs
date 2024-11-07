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
            optionObject.SetActive(false); // 게임 시작 시 일시정지 패널 비활성화
        }
    }

    // 매 프레임마다 호출되는 업데이트 메서드
    void Update()
    {
        // ESC 키를 눌러 게임을 멈추거나 재개
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
        Time.timeScale = 0f; // 게임 시간 정지
        isGamePaused = true;
        if (optionObject != null)
        {
            optionObject.SetActive(true); // 일시정지 UI 활성화
        }
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f; // 게임 시간 재개
        isGamePaused = false;
        if (optionObject != null)
        {
            optionObject.SetActive(false); // 일시정지 UI 비활성화
        }
    }


    public void GameSave()
    {
        dataManager.SaveGame();
    }
}
