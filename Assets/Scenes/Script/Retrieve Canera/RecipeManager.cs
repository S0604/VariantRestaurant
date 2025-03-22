using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    // 儲存已知的料理配方，使用 HashSet 確保不重複
    private HashSet<string> knownRecipes = new HashSet<string>();

    public bool CheckAndSaveNewRecipe(List<IngredientData> ingredients)
    {
        List<string> ingredientNames = new List<string>();
        foreach (var ingredient in ingredients)  // 取得所有食材的名稱
        {
            ingredientNames.Add(ingredient.ingredientName);
        }
        // 排序食材名稱，確保相同的食材組合（不同排列順序）會得到相同的識別碼
        ingredientNames.Sort();

        string recipeKey = string.Join("_", ingredientNames);

        // 判斷這個配方是否已經存在
        if (knownRecipes.Contains(recipeKey))
        {
            return false; // 配方已存在，回傳 false
        }
        else
        {
            knownRecipes.Add(recipeKey); // 新配方，存入紀錄
            return true;// 回傳 true，表示是新料理
        }
    }

    public string GenerateRecipeKey(List<IngredientData> ingredients)
    {
        List<string> ingredientNames = new List<string>();
        foreach (var ingredient in ingredients)// 取得所有食材的名稱
        {
            ingredientNames.Add(ingredient.ingredientName);
        }
        // 排序食材名稱，確保不同順序的相同食材組合產生相同識別碼
        ingredientNames.Sort();
        return string.Join("_", ingredientNames);
    }
}
