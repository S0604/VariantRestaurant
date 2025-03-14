using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class SauceScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public IngredientData ingredientData; // 连接 ScriptableObject
    private Image buttonImage;
    public GameObject nameIconPrefab; // 名称图片的预制体
    private GameObject nameIconInstance;
    private Coroutine hoverCoroutine;

    private void Awake()
    {
        buttonImage = transform.Find("Image")?.GetComponent<Image>();
        if (buttonImage == null)
        {
            Debug.LogError($"❌ {gameObject.name} 找不到 Image 组件，请检查层级结构！");
        }
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

        HideNameIcon();
        Debug.Log($"🖱️ 点击了 {ingredientData.ingredientName}");
    }

    // ====== 【悬停显示名称图片】 ======
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ingredientData?.nameIcon == null)
            return;

        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
        }
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
