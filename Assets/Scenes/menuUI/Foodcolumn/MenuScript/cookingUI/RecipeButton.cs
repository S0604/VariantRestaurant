using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RecipeButton : MonoBehaviour
{
    public RecipeManager recipeManager;
    public GameObject renameUI;
    public GameObject[] uiToCloseOnDuplicate;

    public Button makeRecipeButton;

    public ItemSlot[] ingredientSlots; // 連結 4 個食材格子

    void Start()
    {
        makeRecipeButton.onClick.AddListener(OnMakeRecipeClicked);
    }

    void OnMakeRecipeClicked()
    {
        // 動態收集目前選擇的食材
        List<IngredientData> selectedIngredients = new List<IngredientData>();
        foreach (var slot in ingredientSlots)
        {
            var ingredient = slot.GetIngredientData();
            if (ingredient != null)
            {
                selectedIngredients.Add(ingredient);
            }
        }

        Debug.Log($"🧪 目前選擇的食材數量：{selectedIngredients.Count}");
        foreach (var ing in selectedIngredients)
        {
            Debug.Log($"👉 {ing.ingredientName}");
        }

        // 檢查是否為新配方
        bool isNew = recipeManager.CheckAndSaveNewRecipe(selectedIngredients);
        Debug.Log("是否為新料理: " + isNew);

        if (isNew)
        {
            renameUI.SetActive(true);
        }
        else
        {
            foreach (var ui in uiToCloseOnDuplicate)
            {
                if (ui != null) ui.SetActive(false);
            }
        }
    }
}
