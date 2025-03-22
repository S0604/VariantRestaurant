using System.Collections.Generic;
using UnityEngine;

public class CookingSystem : MonoBehaviour
{
    public RecipeManager recipeManager;
    public BurgerCapture burgerCapture;

    public bool TryStartNewRecipe(List<IngredientData> ingredients)
    {
        if (recipeManager.IsNewRecipe(ingredients))
        {
            return true;
        }
        return false;
    }

    public void SaveAndCapture(List<IngredientData> ingredients)
    {
        recipeManager.SaveRecipe(ingredients);
        string key = recipeManager.GenerateRecipeKey(ingredients);
        burgerCapture.CaptureStackPanel(key);
    }
}