//WASDIconSet ScriptableObject
using UnityEngine;

[CreateAssetMenu(fileName = "WASDIconSet", menuName = "RandomEvent/WASD Icon Set")]
public class WASDIconSetSO : ScriptableObject
{
    public Sprite up;
    public Sprite down;
    public Sprite left;
    public Sprite right;

    public Sprite upCorrect;
    public Sprite downCorrect;
    public Sprite leftCorrect;
    public Sprite rightCorrect;

    public Sprite upWrong;
    public Sprite downWrong;
    public Sprite leftWrong;
    public Sprite rightWrong;
}
