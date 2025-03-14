using UnityEngine;
using UnityEngine.UI;

public class CookingMenuUI : MonoBehaviour
{
    public static CookingMenuUI Instance;

    public Image BottomIngredientImage;  // 对应 ItemSlot(0)
    public Image MiddleIngredientImage;  // 对应 ItemSlot(1)
    public Image TopIngredientImage;     // 对应 ItemSlot(2)
    public Image BottomBunImage;         // 面包底部
    public Image TopBunImage;            // 面包顶部

    public ItemSlot[] itemSlots; // 0:底部, 1:中部, 2:顶部, 3:基底格

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("⚠️ CookingMenuUI: 已有一个实例，可能存在多个对象！");
            Destroy(gameObject);
        }
    }

    public void UpdateStackPanel()
    {
        if (itemSlots.Length < 4)
        {
            Debug.LogError("❌ ItemSlots 数组长度不足！");
            return;
        }

        // 依次获取食材
        IngredientData bottomIngredient = itemSlots[0].GetStoredIngredient();
        IngredientData middleIngredient = itemSlots[1].GetStoredIngredient();
        IngredientData topIngredient = itemSlots[2].GetStoredIngredient();
        IngredientData bunIngredient = itemSlots[3].GetStoredIngredient(); // 基底格（是否是面包）

        // 更新普通食材
        UpdateIngredientImage(BottomIngredientImage, bottomIngredient);
        UpdateIngredientImage(MiddleIngredientImage, middleIngredient);
        UpdateIngredientImage(TopIngredientImage, topIngredient);

        // 处理面包（基底格）
        if (bunIngredient != null && bunIngredient.isBun)
        {
            TopBunImage.sprite = bunIngredient.bunTopImage;
            BottomBunImage.sprite = bunIngredient.bunBottomImage;

            TopBunImage.enabled = true;
            BottomBunImage.enabled = true;
        }
        else
        {
            TopBunImage.sprite = null;
            BottomBunImage.sprite = null;

            TopBunImage.enabled = false;
            BottomBunImage.enabled = false;
        }

        Debug.Log("🍔 StackPanel 更新完成！");
    }

    private void UpdateIngredientImage(Image image, IngredientData ingredient)
    {
        if (ingredient != null)
        {
            image.sprite = ingredient.ingredientImage;
            image.enabled = true;
        }
        else
        {
            image.sprite = null;
            image.enabled = false;
        }
    }
}
