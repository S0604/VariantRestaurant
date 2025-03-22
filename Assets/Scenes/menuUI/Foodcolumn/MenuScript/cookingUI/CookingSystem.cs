using System.Collections.Generic;
using UnityEngine;

public class CookingSystem : MonoBehaviour
{
    public RecipeManager recipeManager; // 料理管理器
    public BurgerCapture burgerCapture; // 截图管理器

    private void Start()
    {
        if (recipeManager == null)
            Debug.LogError("❌ RecipeManager 未绑定！");
        if (burgerCapture == null)
            Debug.LogError("❌ BurgerCapture 未绑定！");
    }

    // 当玩家完成料理时调用
    public void OnRecipeCompleted(List<IngredientData> ingredients)
    {
        if (recipeManager == null || burgerCapture == null)
        {
            Debug.LogError("⚠️ CookingSystem: 组件未正确绑定！");
            return;
        }

        if (recipeManager.CheckAndSaveNewRecipe(ingredients))
        {
            string recipeKey = recipeManager.GenerateRecipeKey(ingredients);
            burgerCapture.CaptureStackPanel(recipeKey); // 传递配方 Key
        }
        else
        {
            Debug.Log("🔄 该料理已存在，不重复截图！");
        }
    }
}
