using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class IngredientButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public IngredientData ingredientData; // 连接 ScriptableObject
    private static ItemSlot selectedSlot;
    private Image buttonImage;
    public GameObject nameIconPrefab; // 名称图片的预制体
    private GameObject nameIconInstance;
    private Coroutine hoverCoroutine;

    private void Awake()
    {
        buttonImage = transform.Find("Image").GetComponent<Image>();
    }

    private void Start()
    {
        if (ingredientData != null && ingredientData.ingredientImage != null)
        {
            buttonImage.sprite = ingredientData.ingredientImage;
            buttonImage.enabled = true;
        }
        else
        {
            buttonImage.enabled = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ingredientData == null)
        {
            Debug.Log("❌ 没有食材数据，无法操作");
            return;
        }

        if (selectedSlot == null)
        {
            Debug.LogWarning("❗ 请先选择一个 ItemSlot");
            return;
        }

        // 记录当前的 hoverHandler
        ItemSlotHoverHandler hoverHandler = selectedSlot.GetComponent<ItemSlotHoverHandler>();

        // **传递 IngredientData 到 ItemSlot**
        selectedSlot.SetItem(ingredientData);

        // **更新 Hover 组件数据**
        hoverHandler?.SetIngredientData(ingredientData);
        hoverHandler?.ForceHideNameIcon();
        HideNameIcon();

        Debug.Log($"✅ 传递 {ingredientData.ingredientName} 到 ItemSlot");

        selectedSlot = null;
    }

    public static void SelectItemSlot(ItemSlot slot)
    {
        selectedSlot = slot;
        Debug.Log($"🎯 已选择 ItemSlot: {slot.gameObject.name}");
    }

    // ====== 【悬停显示名称图片】 ======
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ingredientData?.nameIcon == null)
            return;

        StopAllCoroutines(); // 避免多次启动
        hoverCoroutine = StartCoroutine(ShowNameIconAfterDelay(0.5f));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        hoverCoroutine = null;
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
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                nameIconInstance = Instantiate(nameIconPrefab, canvas.transform);
            }
            else
            {
                Debug.LogError("找不到 Canvas，请确保场景中有一个 Canvas 组件！");
                return;
            }
        }

        if (nameIconInstance != null)
        {
            Image nameImage = nameIconInstance.GetComponent<Image>();
            if (nameImage != null)
            {
                nameImage.sprite = ingredientData.nameIcon;
                nameImage.enabled = true;
            }

            // **确保名称图片维持原始尺寸**
            RectTransform nameIconRect = nameIconInstance.GetComponent<RectTransform>();
            nameIconRect.localScale = Vector3.one; // 确保不被缩放影响
            nameIconRect.sizeDelta = ingredientData.nameIcon.rect.size; // 设为图片原始大小

            // **计算名称图片位置**
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, transform.position); // 转换到屏幕坐标
            Vector2 buttonSize = GetComponent<RectTransform>().sizeDelta; // 获取 IngredientButton 的大小
            Vector2 nameIconPos = screenPos + new Vector2(buttonSize.x / 2 + 122, 96); // 让名称图片显示在右侧

            nameIconRect.position = nameIconPos;
        }
    }

    private void HideNameIcon()
    {
        if (nameIconInstance != null)
        {
            Destroy(nameIconInstance);
            nameIconInstance = null;
        }
    }
}
