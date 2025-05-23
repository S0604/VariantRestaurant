using UnityEngine;
using TMPro;

public class TextUpdater : MonoBehaviour
{
    public TMP_InputField inputField; // 连接 TMP_InputField
    public TMP_Text displayText; // 连接 TMP_Text 组件

    public void UpdateText()
    {
        displayText.text = inputField.text; // 更新文本内容
    }
}
