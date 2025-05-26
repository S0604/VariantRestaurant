using UnityEngine;
using TMPro;

public class ClearInputField : MonoBehaviour
{
    public TMP_InputField inputField;  // 连接 TMP_InputField

    // 这个方法会在按钮点击时调用
    public void ClearText()
    {
        inputField.text = "";  // 清空输入框的文本
    }
}
