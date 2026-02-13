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

    [Header("打字機設定")]
    public float normalTypeSpeed = 0.02f;
    public float fastTypeSpeed = 0.005f;

    private Player player;
    private bool isTyping = false;
    public bool DialogueFinished { get; private set; } = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (dialogueCanvas == null) dialogueCanvas = GetComponentInParent<Canvas>();
        player = FindObjectOfType<Player>();
    }

    /* ---------- 公開入口 ─ 用於外部呼叫 ---------- */
    public IEnumerator PlayDialogueCoroutine()
    {
        currentIndex = 0;
        DialogueFinished = false;
        dialogueCanvas.gameObject.SetActive(true);

        yield return StartCoroutine(PlayDialogue());
    }


    /* ---------- 核心流程：不會受 TimeScale 影響 ---------- */
    IEnumerator PlayDialogue()
    {
        // 凍結世界
        Time.timeScale = 0f;

        // 鎖定玩家
        if (player != null)
        {
            player.isLocked = true;

            if (player.TryGetComponent(out Animator anim))
            {
                anim.updateMode = AnimatorUpdateMode.UnscaledTime; // 🔥關鍵：動畫可動
                anim.SetBool("Ismoving", false);
            }
        }

        // 逐句播放
        while (currentIndex < dialogueData.lines.Length)
        {
            DialogueLine line = dialogueData.lines[currentIndex];

            UpdateUI(line);
            yield return StartCoroutine(TypeText(line.text, line.isLeftSide));

            // 🔥 改成 Unscaled Input
            while (true)
            {
                // 滑鼠點擊
                if (Input.GetMouseButtonDown(0))
                    break;

                // 按住空白鍵自動前進
                if (Input.GetKey(KeyCode.Space))
                    break;

                yield return null;
            }

            currentIndex++;
        }

        // 結尾
        yield return StartCoroutine(EndDialogueChapter());
    }


    /* ---------- 打字機：使用實際時間 ---------- */
    IEnumerator TypeText(string text, bool isLeft)
    {
        isTyping = true;

        leftDialogueText.text = "";
        rightDialogueText.text = "";

        TMP_Text activeText = isLeft ? leftDialogueText : rightDialogueText;
        activeText.gameObject.SetActive(true);
        (isLeft ? rightDialogueText : leftDialogueText).gameObject.SetActive(false);

        text = text.Replace("\\n", "\n");

        foreach (char c in text)
        {
            activeText.text += c;

            float speed = Input.GetKey(KeyCode.Space) ? fastTypeSpeed : normalTypeSpeed;
            yield return new WaitForSecondsRealtime(speed);
        }

        isTyping = false;
    }


    /* ---------- UI 更新 ---------- */
    void UpdateUI(DialogueLine line)
    {
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

        if (line.dialogueBox != null)
        {
            dialogueBoxImage.sprite = line.dialogueBox;
            dialogueBoxImage.color = Color.white;
        }

        if (line.voiceClip != null)
        {
            audioSource.clip = line.voiceClip;
            audioSource.Play();
        }

        leftDialogueText.gameObject.SetActive(line.isLeftSide);
        rightDialogueText.gameObject.SetActive(!line.isLeftSide);
    }


    /* ---------- 結束章節 ---------- */
    private IEnumerator EndDialogueChapter()
    {
        isTyping = false;

        // 恢復時間
        Time.timeScale = 1f;

        // 解鎖玩家
        if (player != null)
            player.isLocked = false;

        // 關閉 UI
        if (dialogueCanvas != null)
            dialogueCanvas.gameObject.SetActive(false);

        DialogueFinished = true;
        yield return null;
    }


    /* ---------- 跳過按鈕 ---------- */
    public void ForceCloseDialogue()
    {
        if (!DialogueFinished)
            StartCoroutine(EndDialogueChapter());
    }
}
