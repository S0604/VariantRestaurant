using UnityEngine;

public class Chapter : MonoBehaviour
{
    [Header("對話章節名稱（對應 Dialogue_XXX.asset）")]
    public string chapterName = "4";   // Inspector 預設 4，可隨意改

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 1. 通知進度（保留原功能）
        TutorialProgressManager.Instance?.CompleteEvent("MobileTeachingCompleted");

        // 2. 播指定章節
        var dlg = FindObjectOfType<TutorialDialogueController>();
        if (dlg != null) dlg.PlayChapter(chapterName);

        // 3. 一次性觸發
        Destroy(gameObject);
    }
}