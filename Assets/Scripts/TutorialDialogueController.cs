using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class TutorialDialogueController : MonoBehaviour
{
    public static TutorialDialogueController Instance;

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

    [System.Serializable]
    public class ChapterEvent
    {
        public string chapterName;
        public UnityEvent onChapterEnd;   // ✅ 對話結束後的事件（可在 Inspector 指派）
    }

    [Header("章節結束事件列表")]
    public List<ChapterEvent> chapterEvents = new List<ChapterEvent>();

    private bool hasPlayedTutorial = false;

    void Start()
    {
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

    IEnumerator StartTutorialSequence()
    {
        dialogueCanvas.gameObject.SetActive(true);

        // ▶ 播放第一章
        yield return PlaySingleChapter(chapter1Name);

        // ▶ 第一章結束後接第二章
        yield return PlaySingleChapter(chapter2Name);

        dialogueCanvas.gameObject.SetActive(false);
        PlayerPrefs.SetInt("HasPlayedTutorial", 1);
        PlayerPrefs.Save();
    }

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

        /* 等 UI 完全關閉後才觸發事件 */
        dialogueCanvas.gameObject.SetActive(false);

        // ✅ 對話「完全結束」才觸發事件（世界已解凍、UI 已關）
        TriggerChapterEndEvent(chapterName);
    }
    public void PlayChapter(string chapterName)
    {
        StartCoroutine(PlaySingleChapter(chapterName));
    }

    private void TriggerChapterEndEvent(string chapterName)
    {
        foreach (var chapterEvent in chapterEvents)
        {
            if (chapterEvent.chapterName == chapterName)
            {
                Debug.Log($"✅ 對話章節 {chapterName} 結束，觸發事件");
                chapterEvent.onChapterEnd?.Invoke();
                break;
            }
        }
    }
}
