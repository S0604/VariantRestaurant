using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [Header("UI 元件")]
    public Image leftPortrait;
    public Image rightPortrait;
    public Image dialogueBoxImage;   // 對話框背景

    [Header("文字框 (左右分開)")]
    public TMP_Text leftDialogueText;   // 左側文字
    public TMP_Text rightDialogueText;  // 右側文字

    [Header("資料來源")]
    public DialogueData dialogueData;
    private int currentIndex = 0;

    private AudioSource audioSource;

    [Header("Canvas 控制")]
    public Canvas dialogueCanvas;   // 👈 這個用來關閉整個對話 UI

    private Player player; // ✅ 玩家控制引用
    private bool isTyping = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // 自動找 Canvas（如果沒手動指定）
        if (dialogueCanvas == null)
            dialogueCanvas = GetComponentInParent<Canvas>();

        // ✅ 自動找到 Player
        player = FindObjectOfType<Player>();

        if (dialogueData != null)
            StartCoroutine(PlayDialogue());
    }

    public IEnumerator PlayDialogue()
    {
        // ✅ 凍結整個世界
        Time.timeScale = 0f;

        // 以下原有程式碼不動 …
        if (player != null)
        {
            player.isLocked = true;
            if (player.TryGetComponent<Animator>(out Animator anim))
                anim.SetBool("Ismoving", false);
        }

        while (currentIndex < dialogueData.lines.Length)
        {
            DialogueLine line = dialogueData.lines[currentIndex];
            UpdateUI(line);
            yield return StartCoroutine(TypeText(line.text, line.isLeftSide));
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
            currentIndex++;
        }

        Debug.Log("📘 對話結束。");

        // ✅ 解凍世界
        Time.timeScale = 1f;

        if (player != null) player.isLocked = false;
        if (dialogueCanvas != null)
        {
            //dialogueCanvas.gameObject.SetActive(false);
            Debug.Log("🎬 DialogueCanvas 已關閉。");
        }
    }
    void UpdateUI(DialogueLine line)
    {
        // 角色立繪切換
        if (line.isLeftSide)
        {
            leftPortrait.sprite = line.portrait;
            leftPortrait.color = Color.white;
            rightPortrait.color = new Color(1, 1, 1, 0f); // 完全透明
        }
        else
        {
            rightPortrait.sprite = line.portrait;
            rightPortrait.color = Color.white;
            leftPortrait.color = new Color(1, 1, 1, 0f); // 完全透明
        }

        // 對話框切換
        if (line.dialogueBox != null)
        {
            dialogueBoxImage.sprite = line.dialogueBox;
            dialogueBoxImage.color = Color.white;
        }

        // 語音播放
        if (line.voiceClip != null)
        {
            audioSource.clip = line.voiceClip;
            audioSource.Play();
        }

        // 顯示哪個文字框
        leftDialogueText.gameObject.SetActive(line.isLeftSide);
        rightDialogueText.gameObject.SetActive(!line.isLeftSide);
    }

    IEnumerator TypeText(string text, bool isLeft)
    {
        isTyping = true;

        text = text.Replace("\\n", "\n");

        TMP_Text activeText = isLeft ? leftDialogueText : rightDialogueText;
        activeText.text = "";

        foreach (char c in text)
        {
            activeText.text += c;
            yield return new WaitForSecondsRealtime(0.02f); // ⬅️ 用真實時間
        }

        isTyping = false;
    }
    public IEnumerator PlayDialogueCoroutine()
    {
        currentIndex = 0;
        yield return StartCoroutine(PlayDialogue());
    }
}
