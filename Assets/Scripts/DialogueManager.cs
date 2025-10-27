using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [Header("UI 元件")]
    public Image leftPortrait;
    public Image rightPortrait;
    public Image dialogueBoxImage;

    [Header("文字框 (左右分開)")]
    public TMP_Text leftDialogueText;
    public TMP_Text rightDialogueText;

    [Header("資料來源")]
    public DialogueData dialogueData;
    private int currentIndex = 0;

    private AudioSource audioSource;
    [Header("Canvas 控制")]
    public Canvas dialogueCanvas;

    private Player player;
    private bool isTyping = false;

    /* ---------- 生命週期 ---------- */
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (dialogueCanvas == null) dialogueCanvas = GetComponentInParent<Canvas>();
        player = FindObjectOfType<Player>();
    }

    /* ---------- 公開入口 ---------- */
    public IEnumerator PlayDialogueCoroutine()
    {
        currentIndex = 0;
        yield return StartCoroutine(PlayDialogue());
    }

    /* ---------- 核心流程 ---------- */
    IEnumerator PlayDialogue()
    {
        /* 1. 凍結世界 */
        Time.timeScale = 0f;

        /* 2. 鎖玩家（可選）*/
        if (player != null)
        {
            player.isLocked = true;
            if (player.TryGetComponent<Animator>(out Animator anim))
                anim.SetBool("Ismoving", false);
        }

        /* 3. 逐句播放 */
        while (currentIndex < dialogueData.lines.Length)
        {
            DialogueLine line = dialogueData.lines[currentIndex];
            UpdateUI(line);
            yield return StartCoroutine(TypeText(line.text, line.isLeftSide));

            // 等待點擊（RealTime 版，確保讀得到輸入）
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
            currentIndex++;
        }

        /* 4. 解凍世界 */
        Time.timeScale = 1f;

        if (player != null) player.isLocked = false;
        if (dialogueCanvas != null) dialogueCanvas.gameObject.SetActive(false);
    }

    /* ---------- 打字協程（RealTime） ---------- */
    IEnumerator TypeText(string text, bool isLeft)
    {
        isTyping = true;
        text = text.Replace("\\n", "\n");

        TMP_Text activeText = isLeft ? leftDialogueText : rightDialogueText;
        activeText.text = "";

        foreach (char c in text)
        {
            activeText.text += c;
            yield return new WaitForSecondsRealtime(0.02f); // ← 關鍵：不受 timeScale 影響
        }

        isTyping = false;
    }

    /* ---------- UI 更新 ---------- */
    void UpdateUI(DialogueLine line)
    {
        // 立繪
        if (line.isLeftSide)
        {
            leftPortrait.sprite = line.portrait;
            leftPortrait.color = Color.white;
            rightPortrait.color = new Color(1, 1, 1, 0f);
        }
        else
        {
            rightPortrait.sprite = line.portrait;
            rightPortrait.color = Color.white;
            leftPortrait.color = new Color(1, 1, 1, 0f);
        }

        // 對話框背景
        if (line.dialogueBox != null)
        {
            dialogueBoxImage.sprite = line.dialogueBox;
            dialogueBoxImage.color = Color.white;
        }

        // 語音
        if (line.voiceClip != null)
        {
            audioSource.clip = line.voiceClip;
            audioSource.Play();
        }

        // 顯示哪一側文字
        leftDialogueText.gameObject.SetActive(line.isLeftSide);
        rightDialogueText.gameObject.SetActive(!line.isLeftSide);
    }
}