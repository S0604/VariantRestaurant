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

        // 额外清除生成的酱料对象
        foreach (GameObject sauce in spawnedSauces)
        {
            Destroy(sauce);
        }
        spawnedSauces.Clear();
    }
    public void UpdateStackPanel()
    {
        if (itemSlots.Length < 4)
        {
            Debug.LogError(" ItemSlots 数组长度不足！");
            return;
        }

        // 1️⃣ 获取所有食材（忽略空槽）
        List<IngredientData> ingredientStack = new List<IngredientData>();

        for (int i = 0; i < 3; i++) // 只处理 0~2（食材格）
        {
            IngredientData ingredient = itemSlots[i].GetStoredIngredient();
            if (ingredient != null)
            {
                ingredientStack.Add(ingredient); // 只存非空食材
            }
        }

        // 2️⃣ 获取基底（是否是面包）
        IngredientData bunIngredient = itemSlots[3].GetStoredIngredient(); // 基底格
        bool isBunBase = bunIngredient != null && bunIngredient.isBun;

        // 3️⃣ 根据食材数量填补空缺
        IngredientData bottomIngredient = ingredientStack.Count > 0 ? ingredientStack[0] : null;
        IngredientData middleIngredient = ingredientStack.Count > 1 ? ingredientStack[1] : null;
        IngredientData topIngredient = ingredientStack.Count > 2 ? ingredientStack[2] : null;

        // 填充 Ingredient 图片
        UpdateIngredientImage(BottomIngredientImage, bottomIngredient);
        UpdateIngredientImage(MiddleIngredientImage, middleIngredient);
        UpdateIngredientImage(TopIngredientImage, topIngredient);

        // 4️⃣ 当基底格有面包时，自动填补空缺
        bool topBunUsedAsFill = false;

        if (isBunBase)
        {
            if (ingredientStack.Count == 0) // 没有食材
            {
                BottomIngredientImage.sprite = bunIngredient.bunTopImage; // ✅ 这里改成 bunTopImage
                BottomIngredientImage.enabled = true;
                topBunUsedAsFill = true;
            }
            else if (ingredientStack.Count == 1 && middleIngredient == null) // 1个食材，填补中层
            {
                MiddleIngredientImage.sprite = bunIngredient.bunTopImage; // ✅ 这里改成 bunTopImage
                MiddleIngredientImage.enabled = true;
                topBunUsedAsFill = true;
            }
            else if (ingredientStack.Count == 2 && topIngredient == null) // 2个食材，填补上层
            {
                TopIngredientImage.sprite = bunIngredient.bunTopImage; // ✅ 这里改成 bunTopImage
                TopIngredientImage.enabled = true;
                topBunUsedAsFill = true;
            }
        }

        // **如果 `TopBunImage` 被填补到其他位置，则隐藏顶部的 `TopBunImage`**
        if (topBunUsedAsFill)
        {
            TopBunImage.sprite = null;
            TopBunImage.enabled = false;
        }
        else if (isBunBase) // **否则正常显示 `TopBunImage` 在顶部**
        {
            TopBunImage.sprite = bunIngredient.bunTopImage;
            TopBunImage.enabled = true;
        }

        // **始终显示底部面包**
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

        Debug.Log(" StackPanel 更新完成！当前食材数量：" + ingredientStack.Count);
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

    private int GetCurrentIngredientCount()
    {
        int count = 0;
        for (int i = 0; i < 3; i++) // 只检查 0~2（食材槽）
        {
            if (itemSlots[i].GetStoredIngredient() != null)
            {
                count++;
            }
        }
        return count;
    }
    private Transform GetSauceParentTransform(int ingredientCount)
    {
        if (ingredientCount == 0) return BottomIngredientImage.transform; //  没有食材，酱料放在底部面包上
        if (ingredientCount == 1) return MiddleIngredientImage.transform; //  只有 1 个食材，酱料放在中层食材上
        if (ingredientCount == 2) return TopIngredientImage.transform;    //  2 个食材，酱料放在顶层食材上

        return null; // 超过 3 层则不再添加酱料
    }
    private int GetSauceInsertIndex()
    {
        int index = 1; // 默认插入在 BottomIngredientImage 之后

        // StackPanel 里一共有 5 个子物体（BottomBunImage、BottomIngredientImage、MiddleIngredientImage、TopIngredientImage、TopBunImage）
        // 我们要找 TopBunImage 的索引，然后往前插入
        for (int i = 0; i < stackPanel.childCount; i++)
        {
            Transform child = stackPanel.GetChild(i);
            if (child.name == "TopBunImage") // 发现了顶部面包
            {
                index = i; // 酱料要插入在 TopBunImage 之前
                break;
            }
        }

        return index; // 返回合适的插入点
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

        // 设定正确的层级
        newSauce.transform.SetSiblingIndex(targetIndex);

        // 存入列表，方便后续清理
        spawnedSauces.Add(newSauce);
    }
}
