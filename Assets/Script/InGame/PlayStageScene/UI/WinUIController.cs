using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class WinUIController : MonoBehaviour
{
    public GameObject winBarBackground;
    public List<GameObject> winUIActiveObjectList;

    private void Start()
    {
        WinUIInit();
    }

    public void WinUIInit()
    {
        winBarBackground.transform.localScale = new Vector3(0f, 0.1f, 1f);
    }

    public void WinUIActive()
    {
        gameObject.SetActive(true);

        StartCoroutine(WinBarAnimation());
    }

    private IEnumerator WinBarAnimation()
    {
        Vector3 scale = winBarBackground.transform.localScale;
        
        // X 스케일 애니메이션
        scale = new Vector3(0f, 0.1f, 1f);
   

        winBarBackground.transform.localScale = scale;
        
        while (scale.x < 1f)
        {
            scale.x += Time.deltaTime * 2.5f;
            winBarBackground.transform.localScale = scale;
            yield return null;
        }
        
        scale.x = 1f;
        
        // Y 스케일 애니메이션 
        winBarBackground.transform.localScale = scale;
        
        while (scale.y < 1f)
        {
            scale.y += Time.deltaTime * 2f; 
            winBarBackground.transform.localScale = scale;
            yield return null;
        }
        
        scale.y = 1f;
        winBarBackground.transform.localScale = scale;

        foreach (GameObject obj in winUIActiveObjectList)
        {
            obj.SetActive(true);
        }
    }
}
