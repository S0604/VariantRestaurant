using UnityEngine;
using TMPro;

public class CardDescriptionUI : MonoBehaviour
{
    public static CardDescriptionUI Instance;

    [Header("UI 元件")]
    public RectTransform panel;
    public TextMeshProUGUI activeSkillText;
    public TextMeshProUGUI passiveSkillText;

    [Header("動畫設定")]
    public float hiddenX = -500f; // 螢幕外的位置
    public float shownX = 100f;   // 顯示時的位置
    public float transitionSpeed = 10f;

    private Vector2 targetPos;

    private void Awake()
    {
        Instance = this;
        targetPos = new Vector2(hiddenX, panel.anchoredPosition.y);
        panel.anchoredPosition = targetPos;
    }

    private void Update()
    {
        panel.anchoredPosition = Vector2.Lerp(panel.anchoredPosition, targetPos, Time.deltaTime * transitionSpeed);
    }

    public void ShowDescription(string activeSkill, string passiveSkill)
    {
        activeSkillText.text = activeSkill;
        passiveSkillText.text = passiveSkill;
        targetPos = new Vector2(shownX, panel.anchoredPosition.y);
    }

    public void HideDescription()
    {
        targetPos = new Vector2(hiddenX, panel.anchoredPosition.y);
    }
}
