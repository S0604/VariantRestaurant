using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RenameUIHandler : MonoBehaviour
{
    public TMP_InputField nameInputField;
    public GameObject renameUI;
    public GameObject newRecipeUI;
    public NewRecipeUI newRecipeUIHandler;
    public CookingSystem cookingSystem;
    private List<IngredientData> currentIngredients;

    public void PrepareRename(List<IngredientData> ingredients)
    {
        currentIngredients = new List<IngredientData>(ingredients);
        nameInputField.text = "";
    }

    public void OnConfirmRename()
    {
        string recipeName = nameInputField.text;
        Debug.Log("命名為: " + recipeName);

        // 保存料理、截圖並顯示新料理 UI
        cookingSystem.SaveAndCapture(currentIngredients);

        renameUI.SetActive(false);
        newRecipeUI.SetActive(true);
        newRecipeUIHandler.SetRecipeName(recipeName);
    }
}
