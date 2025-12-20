using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SimpleLoadingBar : MonoBehaviour
{
    [Header("拖入你的 Slider")]
    public Slider slider;

    void Start()
    {
        if (slider == null)
        {
            Debug.LogError("Slider 尚未拖入！");
            return;
        }

        if (string.IsNullOrEmpty(Globe.nextSceneName))
        {
            Debug.LogError("Globe.nextSceneName 尚未設定！");
            return;
        }

        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(Globe.nextSceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            slider.value = progress;

            if (operation.progress >= 0.9f)
            {
                slider.value = 1f;
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
