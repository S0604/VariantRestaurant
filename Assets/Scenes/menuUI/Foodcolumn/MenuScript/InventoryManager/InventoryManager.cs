using UnityEngine;
using UnityEngine.UI;
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;  // 单例\
    public Transform inventoryPanel;  // 物品栏面板
    private Image selectedIconImage;  // 记录当前选中的 Icon Image

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
        if (inventoryPanel == null)
        {
            Debug.LogError("❌ inventoryPanel 为空！请检查 Unity Inspector 是否正确赋值！");
            return;
        }

        foreach (Transform slotContainer in inventoryPanel)
        {
            Transform itemSlot = slotContainer.Find("ItemSlot");
            if (itemSlot == null)
            {
                Debug.LogWarning($"⚠️ {slotContainer.name} 没有 'ItemSlot'，请检查层级结构！");
                continue;
            }

            Transform iconTransform = itemSlot.Find("Icon");
            if (iconTransform == null)
            {
                Debug.LogWarning($"⚠️ {itemSlot.name} 找不到 'Icon'，请检查预制体结构！");
                continue;
            }

            Image iconImage = iconTransform.GetComponent<Image>();
            if (iconImage == null)
            {
                Debug.LogWarning($"⚠️ {itemSlot.name} 的 'Icon' 没有 Image 组件！");
                continue;
            }

            iconImage.sprite = null; // 清空图片
            iconImage.enabled = false; // 让 Icon 隐藏
        }

        Debug.Log("✅ 物品栏已重置");
    }
 }
