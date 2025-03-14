using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;

public class CookButtonHandler : MonoBehaviour
{
    public CookingSystem cookingSystem; // 引用 CookingSystem 进行逻辑处理
    public GameObject newRecipeUI;      // 用于显示新料理的 UI
    public TMP_Text newRecipeText;          // 显示新料理的名字
    public List<ItemSlot> itemSlots;    // 4个格子，用来存放玩家选择的食材
    private List<IngredientData> selectedIngredients = new List<IngredientData>(); // 玩家选择的食材

    // 绑定到按钮点击事件
    public void OnCookButtonClick()
    {
        // 清空 selectedIngredients 列表
        selectedIngredients.Clear();

        // 检查 itemSlots 是否为空
        if (itemSlots == null || itemSlots.Count == 0)
        {
            Debug.LogError("❌ itemSlots 为空，请在 Inspector 中分配 ItemSlot ！");
            return;
        }

        // 获取每个 ItemSlot 上的食材
        foreach (ItemSlot slot in itemSlots)
        {
            if (slot != null) // 确保 slot 不是 null
            {
                IngredientData ingredient = slot.GetIngredientData();
                if (ingredient != null)
                {
                    selectedIngredients.Add(ingredient);
                }
            }
            else
            {
                Debug.LogWarning("⚠️ ItemSlot 为空！");
            }
        }

        // 如果至少有一个食材被选择
        if (selectedIngredients.Count > 0)
        {
            Debug.Log("🔵 玩家选择了食材，开始检查是否为新料理");
            if (cookingSystem == null)
            {
                Debug.LogError("❌ cookingSystem 为空！请检查 CookButtonHandler 脚本的 Inspector 是否正确绑定了 CookingSystem 对象！");
                return;
            }

            // 调用 CookingSystem 的 OnRecipeCompleted 方法来判断是否为新料理并保存
            cookingSystem.OnRecipeCompleted(selectedIngredients);

            if (newRecipeUI == null)
            {
                Debug.LogError("❌ newRecipeUI 为空！请检查 Inspector 是否正确绑定 UI 元素！");
                return;
            }

            // 显示新料理 UI
            newRecipeUI.SetActive(true);
            newRecipeText.text = "好好吃漢堡";
        }
        else
        {
            Debug.LogWarning("❌ 请选择至少一个食材！");
        }
    }

    // 用来获取新料理的图片，模拟返回新料理图片的逻辑
    private Sprite GetNewRecipeImage()
    {
        // 这里可以放置生成截图的代码
        // 你可以把截图存储到一个变量中并返回
        return null; // 目前返回 null，后续需要补充实际获取截图的逻辑
    }
}
