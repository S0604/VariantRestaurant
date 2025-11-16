using UnityEngine;

public class ResetOnSceneLoad : MonoBehaviour
{
    void Awake()
    {
        // 清除 PlayerPrefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // 重置單例或靜態變數
        ResetStaticData();

        Debug.Log("進入此場景，遊戲已重置！");
    }

    private void ResetStaticData()
    {
        // GameManager.Instance?.ResetData();
        // TutorialProgressManager.Instance?.ResetData();
    }
}
