using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    public Image icon; // UI 图片ItemSlot 的子物件Image
    private ItemSlotHoverHandler hoverHandler;
    private IngredientData currentIngredient; // 当前存放的食材
    private IngredientData storedIngredient;
    private Image slotImage;

    private void Start()
    {
        if (icon == null)
        {
            Debug.LogWarning("⚠️ ItemSlot: icon 未分配，请在 Inspector 中设置。", this);
            return;
        }

        icon.enabled = false;
        hoverHandler = GetComponent<ItemSlotHoverHandler>();
    }
    private void Awake()
    {
        slotImage = GetComponent<Image>();
    }

    public void SetItem(IngredientData ingredient)
    {
        storedIngredient = ingredient;
        icon.enabled = ingredient != null;
        icon.sprite = ingredient?.ingredientImage;


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

        // 通知 CookingMenuUI 更新 StackPanel
        if (CookingMenuUI.Instance != null)
        {
            CookingMenuUI.Instance.UpdateStackPanel();
        }
        else
        {
            Debug.LogError("❌ CookingMenuUI.Instance 为空，无法更新 StackPanel！");
        }

        hoverHandler?.ForceHideNameIcon();
        (CookingMenuUI.Instance as CookingMenuUI_Extended)?.CheckIngredientsAndToggleUI();
 
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        IngredientButton.SelectItemSlot(this);
        Debug.Log("🔵 选中了一个 ItemSlot");
        hoverHandler?.ForceHideNameIcon();
    }

    // 获取当前食材数据
    public IngredientData GetIngredientData()
    {
        return currentIngredient;
    }

    // 获取存储的食材数据
    public IngredientData GetStoredIngredient()
    {
        return storedIngredient;
    }
    public void SetIngredient(IngredientData data)
    {
        currentIngredient = data;
        UpdateUI();
    }

    public void ClearSlot()
    {
        currentIngredient = null;
        storedIngredient = null; // 确保存储的数据也被清除
        icon.sprite = null;
        icon.enabled = false;

        hoverHandler?.ClearStoredIngredient();

        Debug.Log("❌ ItemSlot: 已清空所有数据");
    }

    private void UpdateUI()
    {
        if (currentIngredient != null)
        {
            slotImage.sprite = currentIngredient.ingredientImage;
            slotImage.enabled = true;
        }
        else
        {
            slotImage.enabled = false;
        }
    }
}