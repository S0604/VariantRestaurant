using UnityEngine;

[System.Serializable]
public class RandomEvent
{
    public string eventName;

    [Header("���q�ϥ�")]
    public Sprite upIcon;
    public Sprite downIcon;
    public Sprite leftIcon;
    public Sprite rightIcon;

    [Header("���T�ϥ�")]
    public Sprite upCorrectIcon;
    public Sprite downCorrectIcon;
    public Sprite leftCorrectIcon;
    public Sprite rightCorrectIcon;

    [Header("���~�ϥ�")]
    public Sprite upWrongIcon;
    public Sprite downWrongIcon;
    public Sprite leftWrongIcon;
    public Sprite rightWrongIcon;

    [Header("�I��")]
    public Sprite background;
}
