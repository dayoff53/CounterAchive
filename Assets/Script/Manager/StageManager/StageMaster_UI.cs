using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public partial class BattleStageMaster
{
    #region UIVariable

    [Space(20)]
    [Header("------------------- UI -------------------")]
    
        [Space(10)]
        [Header("UI Object")]
        
        /// <summary>
        /// 플레이 중인 UI
        /// </summary>
        [SerializeField]
        private PlayUIController playUIController;
        [SerializeField]
        private GameObject play_UI;

        /// <summary>
        /// 게임 초기 유닛 배치 중인 UI
        /// </summary>
        [SerializeField]
        private UnitSetUIController unitSetUIController;
        [SerializeField]
        private GameObject unitSet_UI;

        /// <summary>
        /// 승리 중인 UI
        /// </summary>
        [SerializeField]
        private WinUIController winUIController;
        [SerializeField]
        private GameObject win_UI;


        /// <summary>
        /// StageUI 우측 하단에 위치하는 각종 데이터들을 표기하는 공간의 컴포넌트
        /// </summary>
        public StageWindowController stageMenuController;

        /// <summary>
        /// 페이드 인엔아웃 기능을 제공하는 이미지
        /// </summary>
        public Image fadeProdutionPanel;

        [Space(10)]
        [Header("Color Data")]
        public List<Color> unitStateColors;
        public ProdutionState unitStateColorsObject;



    #endregion

/// 페이드 인 아웃 효과
/// </summary>
/// <param name="color">페이드 색상</param>
/// <param name="fadeTime">페이드 시간</param>
    public void SetFadeInOutProduction(Color color, float fadeTime)
    {
        StartCoroutine(FadeInOutProduction(color, fadeTime));
    }
    
    IEnumerator FadeInOutProduction(Color color, float fadeTime)
    {
        float elapsedTime = 0f;
        Color startColor = fadeProdutionPanel.color;


        if(fadeTime == 0)
        {
            fadeProdutionPanel.color = color;
            yield break;
        }

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeTime;
            fadeProdutionPanel.color = Color.Lerp(startColor, color, t);
            yield return new WaitForSeconds(Time.deltaTime);
        }

        fadeProdutionPanel.color = color;
    }

        public void SwitchUIMode(bool isPlayMode)
        {
            play_UI.SetActive(isPlayMode);
            unitSet_UI.SetActive(!isPlayMode);
            
            if(!isPlayMode)
            {
                win_UI.SetActive(false);
            }
        }

}
