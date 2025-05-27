using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcherLoading : MonoBehaviour
{
    public void SwitchToLoading(string targetSceneName)
    {
        Globe.nextSceneName = targetSceneName;
        SceneManager.LoadScene("Loading");
    }
}
