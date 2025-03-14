using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    private HashSet<string> knownRecipes = new HashSet<string>();

    // ???????????
    public bool CheckAndSaveNewRecipe(List<IngredientData> ingredients)
    {
        // ?????????????????
        List<string> ingredientNames = new List<string>();
        foreach (var ingredient in ingredients)
        {
            ingredientNames.Add(ingredient.ingredientName);
        }

        // ???????????????
        ingredientNames.Sort();

        string recipeKey = string.Join("_", ingredientNames);

        // ?????????
        if (knownRecipes.Contains(recipeKey))
        {
            return false; // ???????
        }
        else
        {
            knownRecipes.Add(recipeKey); // ???????
            return true;
        }
    }

    // ???????Key??? "?_?_??"?
    public string GenerateRecipeKey(List<IngredientData> ingredients)
    {
        List<string> ingredientNames = new List<string>();
        foreach (var ingredient in ingredients)
        {
            ingredientNames.Add(ingredient.ingredientName);
        }
        ingredientNames.Sort();
        return string.Join("_", ingredientNames);
    }
}
