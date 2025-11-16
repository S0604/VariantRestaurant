using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PreloadScene : MonoBehaviour
{
    IEnumerator Preload(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        yield return asyncLoad;
        // 預加載完成，隨時可切換
    }

    void Start()
    {
        StartCoroutine(Preload("GameScene 1"));
    }
}
