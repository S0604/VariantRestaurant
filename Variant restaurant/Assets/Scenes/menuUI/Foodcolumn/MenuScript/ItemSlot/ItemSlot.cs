using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    private ItemSlotHoverHandler hoverHandler;
    private IngredientData currentIngredient; // 存储完整的食材数据
    private IngredientData storedIngredient;

    private void Start()
    {
        if (icon == null)
        {
            Debug.LogWarning("ItemSlot: icon 未分配，请在 Inspector 中设置。", this);
            return;
        }

        icon.enabled = false;
        hoverHandler = GetComponent<ItemSlotHoverHandler>();
    }

    public void SetItem(IngredientData ingredient)
    {
        storedIngredient = ingredient;
        icon.enabled = ingredient != null;
        icon.sprite = ingredient?.ingredientImage;

        // 通知 StackPanel 更新食材显示
        CookingMenuUI.Instance.UpdateStackPanel();

        if (ingredient != null)
        {
            currentIngredient = ingredient;
            icon.sprite = ingredient.ingredientImage;
            icon.enabled = true;
            Debug.Log($"✅ ItemSlot 设定食材: {ingredient.ingredientName}");
        }
        else
        {
            currentIngredient = null;
            icon.sprite = null;
            icon.enabled = false;
            Debug.Log("❌ ItemSlot 取消食材");
        }

        hoverHandler?.ForceHideNameIcon();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        IngredientButton.SelectItemSlot(this);
        Debug.Log("🔵 选中了一个 ItemSlot");
        hoverHandler?.ForceHideNameIcon();
    }

    public IngredientData GetIngredientData()
    {
        return currentIngredient;
    }
    public IngredientData GetStoredIngredient()
    {
        return storedIngredient;
    }
}
