using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // 編號切換場景
    public void SwitchSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}