using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class SauceButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public IngredientData ingredientData; // 连接 ScriptableObject
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
            Debug.Log("❌ 没有酱料数据，无法操作");
            return;
        }

        CookingMenuUI.Instance.SelectSauce(ingredientData);
        Debug.Log($"✅ 选择酱料: {ingredientData.ingredientName}");
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
            Transform canvasTransform = GameObject.Find("Cooking UI (Hamburger)").transform;
            nameIconInstance = Instantiate(nameIconPrefab, canvasTransform);
        }

        if (nameIconInstance != null)
        {
            Image nameImage = nameIconInstance.GetComponent<Image>();
            if (nameImage != null)
            {
                nameImage.sprite = ingredientData.nameIcon;
                nameImage.enabled = true;
            }

            // 确保名称图片维持原始尺寸
            RectTransform nameIconRect = nameIconInstance.GetComponent<RectTransform>();
            nameIconRect.localScale = Vector3.one;
            nameIconRect.sizeDelta = ingredientData.nameIcon.rect.size;

            // 计算名称图片位置（ItemSlot 右侧）
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, transform.position);
            Vector2 slotSize = GetComponent<RectTransform>().sizeDelta;
            float nameIconWidth = nameIconRect.sizeDelta.x;
            Vector2 nameIconPos = screenPos + new Vector2(slotSize.x / 2 + 122, 96);
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
