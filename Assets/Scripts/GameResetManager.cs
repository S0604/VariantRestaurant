using UnityEngine;
using UnityEngine.SceneManagement;

public class GameResetManager : MonoBehaviour
{
    /// <summary>
    /// 一鍵重置遊戲
    /// </summary>
    public void ResetGame()
    {
        // 1️⃣ 清除 PlayerPrefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs 已清除！");

        // 2️⃣ 重置靜態變數或單例（視你的遊戲需求）
        ResetStaticData();

        // 3️⃣ 重新載入當前場景
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// 如果你有靜態變數或單例，這裡統一重置
    /// </summary>
    private void ResetStaticData()
    {
        // 範例：假設你的單例有一個 Reset 方法
        // GameManager.Instance?.ResetData();
        // TutorialProgressManager.Instance?.ResetData();

        Debug.Log("靜態資料已重置（如有）。");
    }
}
