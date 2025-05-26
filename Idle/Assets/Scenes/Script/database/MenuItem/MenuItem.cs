using UnityEngine;

[CreateAssetMenu(fileName = "New Menu Item", menuName = "Menu/Menu Item")]
public class MenuItem : ScriptableObject
{
    public string itemName;
    public Sprite itemImage;
    public string itemTag;
}
