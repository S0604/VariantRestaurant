using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "Menu Database", menuName = "Menu/Menu Database")]
public class MenuDatabase : ScriptableObject
{
    public static MenuDatabase Instance;

    [Header("所有料理資料")]
    public MenuItem[] allMenuItems;

    private void OnEnable()
    {
        Instance = this;
    }

    /// <summary>
    /// 根據料理標籤（itemTag）取得對應的 MenuItem
    /// </summary>
    public MenuItem GetMenuItemByTag(string tag)
    {
        if (allMenuItems == null || allMenuItems.Length == 0)
        {
            Debug.LogWarning("MenuDatabase：目前沒有任何料理資料！");
            return null;
        }

        return allMenuItems.FirstOrDefault(item => item.itemTag == tag);
    }
}
