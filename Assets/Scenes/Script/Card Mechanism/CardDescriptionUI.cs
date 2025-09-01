using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardDescriptionUI : MonoBehaviour
{
    public static CardDescriptionUI Instance;

    [Header("UI Panel")]
    public RectTransform panel;  // 整個描述 UI 父物件

    [Header("主動技能 UI")]
    public Image activeSkillPanel;
    public TextMeshProUGUI activeSkillNameText;
    public TextMeshProUGUI activeSkillDescText;

    [Header("被動技能 UI")]
    public Image passiveSkillPanel;
    public TextMeshProUGUI passiveSkillNameText;
    public TextMeshProUGUI passiveSkillDescText;

    [Header("動畫設定")]
    public float hiddenX = -500f; // 隱藏時位置
    public float shownX = 100f;   // 顯示時位置
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

    public void ShowDescription(string activeName, string activeDesc, string passiveName, string passiveDesc)
    {
        // 主動技能 UI
        if (!string.IsNullOrEmpty(activeName) || !string.IsNullOrEmpty(activeDesc))
        {
            activeSkillPanel.gameObject.SetActive(true);
            activeSkillNameText.text = activeName;
            activeSkillDescText.text = activeDesc;
        }
        else
        {
            activeSkillPanel.gameObject.SetActive(false);
        }

        // 被動技能 UI
        if (!string.IsNullOrEmpty(passiveName) || !string.IsNullOrEmpty(passiveDesc))
        {
            passiveSkillPanel.gameObject.SetActive(true);
            passiveSkillNameText.text = passiveName;
            passiveSkillDescText.text = passiveDesc;
        }
        else
        {
            passiveSkillPanel.gameObject.SetActive(false);
        }

        // 滑入
        targetPos = new Vector2(shownX, panel.anchoredPosition.y);
    }

    public void HideDescription()
    {
        targetPos = new Vector2(hiddenX, panel.anchoredPosition.y);
    }
}
