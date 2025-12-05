using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class TutorialDialogueController : MonoBehaviour
{
    public static TutorialDialogueController Instance;
    private FreeModeToggleManager modeManager;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        modeManager = FreeModeToggleManager.Instance;
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
        // ▶ 先檢查教學模式是否啟用
        if (modeManager != null && !modeManager.TutorialModeActive)
        {
            dialogueCanvas.gameObject.SetActive(false);
            yield break; // 教學模式沒開啟就直接跳過
        }

        dialogueCanvas.gameObject.SetActive(true);

        // ▶ 播放第一章
        yield return PlaySingleChapter(chapter1Name);

        // ▶ 第一章結束後接第二章
        yield return PlaySingleChapter(chapter2Name);

        dialogueCanvas.gameObject.SetActive(false);

        // 標記已播放過教學
        PlayerPrefs.SetInt("HasPlayedTutorial", 1);
        PlayerPrefs.Save();

        // 如果有 FreeModeToggleManager，結束教學後啟用時間流逝
        if (modeManager != null)
            modeManager.DisableTutorialMode(); // 教學結束，自由營業開始
    }

    public IEnumerator PlaySingleChapter(string chapterName)
    {
        // 如果教學模式沒開啟，直接跳過
        if (modeManager != null && !modeManager.TutorialModeActive)
            yield break;

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

        dialogueCanvas.gameObject.SetActive(false);

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
