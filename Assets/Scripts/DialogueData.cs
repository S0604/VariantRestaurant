using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueData", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("本章節的所有對話行")]
    public DialogueLine[] lines;
}
