using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [Header("角色資訊")]
    public string characterName; // 不再顯示名字，但保留作識別
    public bool isLeftSide = true;

    [Header("對話內容")]
    [TextArea(2, 5)]
    public string text;

    [Header("視覺與音效")]
    public Sprite portrait;       // 人物立繪
    public AudioClip voiceClip;   // 聲音檔
    public Sprite dialogueBox;    // ✅ 新增：角色專用對話框
}
