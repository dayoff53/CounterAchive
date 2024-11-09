using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
