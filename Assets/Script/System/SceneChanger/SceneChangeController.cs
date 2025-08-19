using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;



[InfoBox("SceneChangeManager를 호출하여 Scene을 변경시키는 스크립트.")]
public class SceneChangerController : MonoBehaviour
{
    public void SceneChange(SceneKeyData sceneKeyData)
    {
        SceneChangeManager.Instance.SceneLoad(sceneKeyData);
    }

    public void ResetScene()
    {
        SceneChangeManager.Instance.SelfSceneLoad();
    }
}
