using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New ColorState", menuName = "Datas/ColorState", order = 0)]
[System.Serializable]


public class ProdutionState : ScriptableObject
{
    [Header("색상 상태(디폴트, 1팀(플레이어), 2팀(적), 3팀(AI 아군), 4팀(제2의 적), 5팀, 6팀, Null)")]
    public List<Color> colorStates;

    [Header("레이어 순서 (디폴트, 페이드 아웃 효과 앞 레이어)")]
    public List<int> orderLayerNumber;
}
