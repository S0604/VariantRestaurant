using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    private HashSet<string> knownRecipes = new HashSet<string>();

    public bool IsNewRecipe(List<IngredientData> ingredients)
    {
        string key = GenerateRecipeKey(ingredients);
        return !knownRecipes.Contains(key);
    }

    public void SaveRecipe(List<IngredientData> ingredients)
    {
        string key = GenerateRecipeKey(ingredients);
        knownRecipes.Add(key);
    }

    public string GenerateRecipeKey(List<IngredientData> ingredients)
    {
        List<string> names = new List<string>();
        foreach (var ingredient in ingredients)
        {
            names.Add(ingredient.ingredientName);
        }
        names.Sort();
        return string.Join("_", names);
    }
}