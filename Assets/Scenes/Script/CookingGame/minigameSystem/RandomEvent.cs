using UnityEngine;

[System.Serializable]
public class RandomEvent
{
    public string eventName;

    [Header("普通圖示")]
    public Sprite upIcon;
    public Sprite downIcon;
    public Sprite leftIcon;
    public Sprite rightIcon;

    [Header("正確圖示")]
    public Sprite upCorrectIcon;
    public Sprite downCorrectIcon;
    public Sprite leftCorrectIcon;
    public Sprite rightCorrectIcon;

    [Header("錯誤圖示")]
    public Sprite upWrongIcon;
    public Sprite downWrongIcon;
    public Sprite leftWrongIcon;
    public Sprite rightWrongIcon;

    [Header("背景")]
    public Sprite background;
}
