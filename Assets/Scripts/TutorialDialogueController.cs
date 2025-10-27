using UnityEngine;
using System.Collections;

public class TutorialDialogueController : MonoBehaviour
{
    public static TutorialDialogueController Instance;   // ➜ 新增單例

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Header("References")]
    public DialogueManager dialogueManager;
    public Canvas dialogueCanvas;

    [Header("Dialogue Chapters")]
    public string chapter1Name = "Chapter1";
    public string chapter2Name = "Chapter2";

    private bool hasPlayedTutorial = false;
    void Start()
    {
        // ✅ 檢查是否為第一次進入遊戲
        hasPlayedTutorial = PlayerPrefs.GetInt("HasPlayedTutorial", 0) == 1;

        if (!hasPlayedTutorial)
        {
            StartCoroutine(StartTutorialSequence());
        }
        else
        {
            dialogueCanvas.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 教學流程：第一章播完 → 自動接第二章
    /// </summary>
    IEnumerator StartTutorialSequence()
    {
        dialogueCanvas.gameObject.SetActive(true);

        // ▶ 播放第一章
        yield return PlaySingleChapter(chapter1Name);

        // ▶ 第一章結束後自動播放第二章
        yield return PlaySingleChapter(chapter2Name);

        // ✅ 教學完成
        dialogueCanvas.gameObject.SetActive(false);
        PlayerPrefs.SetInt("HasPlayedTutorial", 1);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 播放單一章節（外部或內部都可呼叫）
    /// </summary>
    public IEnumerator PlaySingleChapter(string chapterName)
    {
        dialogueCanvas.gameObject.SetActive(true);

        string path = $"DialogueData/Dialogue_{chapterName}";
        DialogueData data = Resources.Load<DialogueData>(path);
        if (data == null)
        {
            Debug.LogError($"❌ 找不到對話檔：{path}");
            yield break;
        }

        dialogueManager.dialogueData = data;
        yield return dialogueManager.StartCoroutine(dialogueManager.PlayDialogueCoroutine());

        // 可選：播放結束後暫時關閉 Canvas（視情況而定）
        dialogueCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// 供外部呼叫播放任意章節（例如觸發第三章）
    /// </summary>
    public void PlayChapter(string chapterName)
    {
        StartCoroutine(PlaySingleChapter(chapterName));
    }
}
