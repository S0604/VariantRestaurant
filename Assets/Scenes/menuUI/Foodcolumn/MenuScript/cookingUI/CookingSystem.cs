using System.Collections.Generic;
using UnityEngine;

public class CookingSystem : MonoBehaviour
{
    public RecipeManager recipeManager; // 料理管理器
    public BurgerCapture burgerCapture; // 截图管理器

    // 当玩家完成料理时调用
    public void OnRecipeCompleted(List<IngredientData> ingredients)
    {
        if (recipeManager.CheckAndSaveNewRecipe(ingredients))
        {
            // 若是新料理，则截图并显示
            string recipeKey = recipeManager.GenerateRecipeKey(ingredients).Replace(",", "_");
            burgerCapture.CaptureStackPanel(); // 进行截图
        }
        else
        {
            Debug.Log("🔄 该料理已存在，不重复截图！");
        }
    }
}
