using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CookingMenuUI : MonoBehaviour
{
    public static CookingMenuUI Instance;

    [SerializeField] private Transform stackPanel; // 在 Inspector 里手动赋值
    public Image TopBunImage;            // 面包顶部
    public Image TopIngredientImage;     // 对应 ItemSlot(2)
    public Image MiddleIngredientImage;  // 对应 ItemSlot(1)
    public Image BottomIngredientImage;  // 对应 ItemSlot(0)
    public Image BottomBunImage;         // 面包底部
    private List<GameObject> spawnedSauces = new List<GameObject>(); // 存放已生成的酱料对象

    public ItemSlot[] itemSlots; // 0:底部, 1:中部, 2:顶部, 3:基底格

    private List<IngredientData> ingredientStack = new List<IngredientData>(); // 存放当前食材

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning(" CookingMenuUI: 已有一个实例，可能存在多个对象！");
            Destroy(gameObject);
        }
    }

    public void CloseUI()
    {
        foreach (var slot in itemSlots)
        {
            slot.ClearSlot(); // 确保所有 ItemSlot 数据被清除
        }
        gameObject.SetActive(false);
    }

    public void OpenUI()
    {
        gameObject.SetActive(true);
        ClearAllIngredientData(); // **清除所有存储的食材数据**
        UpdateStackPanel();
        foreach (var slot in itemSlots)
        {
            slot.ClearSlot(); // 重新打开时确保 slot 为空
        }
    }

    private void ClearAllIngredientData()
    {
        foreach (var slot in itemSlots)
        {
            slot.ClearSlot();
            var hoverHandler = slot.GetComponent<ItemSlotHoverHandler>();
            hoverHandler?.ClearStoredIngredient();
        }

        foreach (GameObject sauce in spawnedSauces)
        {
            Destroy(sauce);
        }
        spawnedSauces.Clear();
    }

    public void UpdateStackPanel()
    {
        ingredientStack.Clear(); // 清空旧数据

        if (itemSlots.Length < 4)
        {
            Debug.LogError(" ItemSlots 数组长度不足！");
            return;
        }

        for (int i = 0; i < 3; i++) // 只处理 0~2（食材格）
        {
            IngredientData ingredient = itemSlots[i].GetStoredIngredient();
            if (ingredient != null)
            {
                ingredientStack.Add(ingredient);
            }
        }

        IngredientData bunIngredient = itemSlots[3].GetStoredIngredient(); // 基底格
        bool isBunBase = bunIngredient != null && bunIngredient.isBun;

        IngredientData bottomIngredient = ingredientStack.Count > 0 ? ingredientStack[0] : null;
        IngredientData middleIngredient = ingredientStack.Count > 1 ? ingredientStack[1] : null;
        IngredientData topIngredient = ingredientStack.Count > 2 ? ingredientStack[2] : null;

        UpdateIngredientImage(BottomIngredientImage, bottomIngredient);
        UpdateIngredientImage(MiddleIngredientImage, middleIngredient);
        UpdateIngredientImage(TopIngredientImage, topIngredient);

        bool topBunUsedAsFill = false;

        if (isBunBase)
        {
            if (ingredientStack.Count == 0)
            {
                BottomIngredientImage.sprite = bunIngredient.bunTopImage;
                BottomIngredientImage.enabled = true;
                topBunUsedAsFill = true;
            }
            else if (ingredientStack.Count == 1 && middleIngredient == null)
            {
                MiddleIngredientImage.sprite = bunIngredient.bunTopImage;
                MiddleIngredientImage.enabled = true;
                topBunUsedAsFill = true;
            }
            else if (ingredientStack.Count == 2 && topIngredient == null)
            {
                TopIngredientImage.sprite = bunIngredient.bunTopImage;
                TopIngredientImage.enabled = true;
                topBunUsedAsFill = true;
            }
        }

        if (topBunUsedAsFill)
        {
            TopBunImage.sprite = null;
            TopBunImage.enabled = false;
        }
        else if (isBunBase)
        {
            TopBunImage.sprite = bunIngredient.bunTopImage;
            TopBunImage.enabled = true;
        }

        if (isBunBase)
        {
            BottomBunImage.sprite = bunIngredient.bunBottomImage;
            BottomBunImage.enabled = true;
        }
        else
        {
            BottomBunImage.sprite = null;
            BottomBunImage.enabled = false;
        }
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
            image.sprite = default;
            image.enabled = false;
        }
    }

    public void SelectSauce(IngredientData sauceData)
    {
        if (sauceData == null || sauceData.saucePrefab == null)
        {
            Debug.LogWarning("⚠️ 选择的酱料无效或 saucePrefab 为空！");
            return;
        }

        Debug.Log($"🎉 选择了酱料: {sauceData.ingredientName}");

        // 计算酱料的插入索引
        int targetIndex = GetSauceInsertIndex();

        // 生成酱料，并设定父对象为 stackPanel
        GameObject newSauce = Instantiate(sauceData.saucePrefab, stackPanel);
        newSauce.transform.SetSiblingIndex(targetIndex);

        // **调整 Y 轴位置**
        RectTransform sauceRect = newSauce.GetComponent<RectTransform>();
        if (sauceRect != null)
        {
            float yOffset = GetSauceYOffset();
            Vector3 newPosition = sauceRect.anchoredPosition;
            newPosition.y += yOffset;
            sauceRect.anchoredPosition = newPosition;
        }

        // 存入列表，方便后续清理
        spawnedSauces.Add(newSauce);
    }

    private int GetSauceInsertIndex()
    {
        int ingredientCount = ingredientStack.Count;

        if (ingredientCount == 1)
        {
            return MiddleIngredientImage.transform.GetSiblingIndex();
        }
        else if (ingredientCount == 2)
        {
            return TopIngredientImage.transform.GetSiblingIndex();
        }
        else if (ingredientCount == 3)
        {
            return TopBunImage.transform.GetSiblingIndex();
        }

        return stackPanel.childCount;
    }
    private float GetSauceYOffset()
    {
        int ingredientCount = ingredientStack.Count; // 直接使用 ingredientStack 计算食材数量

        switch (ingredientCount)
        {
            case 0: return 0f;   // 没有食材，保持默认位置
            case 1: return -40f;  // 1 个食材，酱料稍微上移
            case 2: return -20f;  // 2 个食材，酱料更高
            case 3: return 0f; // 3 个食材，酱料放在最高层
            default: return 0f;  // 兜底情况
        }
    }
}
