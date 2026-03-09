using UnityEngine;

public class TutorialEndOnDestroy : MonoBehaviour
{
    private void OnDestroy()
    {
        // 確保 FreeModeToggleManager 存在
        if (FreeModeToggleManager.Instance != null)
        {
            // 啟動協程播放 Chapter 13（延遲 0 秒）
            FreeModeToggleManager.Instance.StartCoroutine(
                FreeModeToggleManager.Instance.PlayChapterAfterDelay("13", 0f)
            );

            Debug.Log("Chapter 13 將播放，結束教學後時間流動將啟用。");
        }
    }
}