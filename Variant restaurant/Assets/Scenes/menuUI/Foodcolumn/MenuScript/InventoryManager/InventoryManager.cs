using UnityEngine;
using UnityEngine.UI;
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;  // 单例\


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;  // 赋值为单例
        }
        else
        {
            Destroy(gameObject);  // 如果已有实例，销毁这个对象
        }
    }

    public Transform inventoryPanel;  // 物品栏面板
    private Image selectedIconImage;  // 记录当前选中的 Icon Image

    public void SelectSlot(Image iconImage)
    {
        selectedIconImage = iconImage;  // 保存当前选中的 Icon Image
        Debug.Log("✅ 选中了一个 ItemSlot");
    }

    public void AddToInventory(Sprite ingredientSprite)
    {
        if (selectedIconImage != null)
        {
            selectedIconImage.sprite = ingredientSprite;  // 替换选中的格子
            selectedIconImage.enabled = true;  // 确保 Icon 显示
            Debug.Log($"✅ 替换 Icon: {ingredientSprite.name}");
        }
        else
        {
            Debug.LogError("❌ 没有选中任何 ItemSlot！");
        }
    }

    public void ResetInventory()
    {
        foreach (Transform slot in inventoryPanel)
        {
            Image iconImage = slot.Find("Icon").GetComponent<Image>(); // 获取 Icon 的 Image 组件
            iconImage.sprite = null; // 清空图片
            iconImage.enabled = false; // 让 Icon 隐藏
        }
        Debug.Log("✅ 物品栏已重置");
    }
}
