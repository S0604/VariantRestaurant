using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ItemSlotHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private ItemSlot itemSlot;
    private IngredientData storedIngredient; // 存储来自 IngredientButton 的食材数据
    public GameObject nameIconPrefab; // 名称图片的预制体
    private GameObject nameIconInstance;
    private Coroutine hoverCoroutine;

    private void Awake()
    {
        itemSlot = GetComponent<ItemSlot>();
    }

    public void SetIngredientData(IngredientData ingredientData)
    {
        storedIngredient = ingredientData;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (storedIngredient == null || storedIngredient.nameIcon == null)
            return;

        hoverCoroutine = StartCoroutine(ShowNameIconAfterDelay(0.5f));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }

        HideNameIcon();
    }

    private IEnumerator ShowNameIconAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowNameIcon();
    }

    private void ShowNameIcon()
    {
        if (nameIconInstance == null && nameIconPrefab != null)
        {
            Transform canvasTransform = GameObject.Find("Cooking UI (Hamburger)").transform;
            nameIconInstance = Instantiate(nameIconPrefab, canvasTransform);
        }

        if (nameIconInstance != null)
        {
            Image nameImage = nameIconInstance.GetComponent<Image>();
            if (nameImage != null)
            {
                nameImage.sprite = storedIngredient.nameIcon;
                nameImage.enabled = true;
            }

            // 确保名称图片维持原始尺寸
            RectTransform nameIconRect = nameIconInstance.GetComponent<RectTransform>();
            nameIconRect.localScale = Vector3.one;
            nameIconRect.sizeDelta = storedIngredient.nameIcon.rect.size;

            // 计算名称图片位置（ItemSlot 右侧）
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, transform.position);
            Vector2 slotSize = GetComponent<RectTransform>().sizeDelta;
            float nameIconWidth = nameIconRect.sizeDelta.x;
            Vector2 nameIconPos = screenPos + new Vector2(slotSize.x / 2 + 122, 96); // **讓名稱圖片顯示在右側 10px 處**
            nameIconRect.position = nameIconPos;
        }
    }

    public void HideNameIcon()
    {
        if (nameIconInstance != null)
        {
            Destroy(nameIconInstance);
            nameIconInstance = null;
        }
    }

    // 新增一个公共方法，可以在外部手动调用，确保 ItemSlot 关闭时隐藏名称图片
    public void ForceHideNameIcon()
    {
        StopHoverCoroutine();
        HideNameIcon();
    }

    // 取消悬停计时的协程
    private void StopHoverCoroutine()
    {
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }
    }
    public void ClearStoredIngredient()
    {
        storedIngredient = null;
        ForceHideNameIcon();
    }

}
