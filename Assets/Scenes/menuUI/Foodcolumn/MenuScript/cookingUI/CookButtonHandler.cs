using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CookButtonHandler : MonoBehaviour
{
    public CookingSystem cookingSystem;
    public GameObject renameUI;
    public RenameUIHandler renameUIHandler;
    public ClearIngredientData clearIngredientData;
    public List<ItemSlot> itemSlots;
    private List<IngredientData> selectedIngredients = new List<IngredientData>();
    public List<GameObject> uiToCloseOnOldRecipe; // 在 Inspector 裡拖 UI 進來
    public List<GameObject> uiToCloseOnNewRecipe;


    private void CloseAllUIFromList()
    {
        foreach (GameObject ui in uiToCloseOnOldRecipe)
        {
            if (ui != null)
            {
                ui.SetActive(false);
            }
        }
    }
    private void CloseNewRecipeUI()
    {
        foreach (GameObject ui in uiToCloseOnNewRecipe)
        {
            if (ui != null)
            {
                ui.SetActive(false);
            }
        }
    }

    public void OnCookButtonClick()
    {
        selectedIngredients.Clear();

        foreach (ItemSlot slot in itemSlots)
        {
            IngredientData data = slot.GetIngredientData();
            if (data != null) selectedIngredients.Add(data);
        }

        if (selectedIngredients.Count == 0)
        {
            Debug.LogWarning("未選擇任何食材");
            return;
        }

        if (cookingSystem.TryStartNewRecipe(selectedIngredients))
        {
            CloseNewRecipeUI(); // <== 隱藏新料理要關的 UI
            renameUI.SetActive(true);
            renameUIHandler.PrepareRename(selectedIngredients);
        }

        else
        {
            Debug.Log("舊料理，不顯示 UI");
            CloseAllUIFromList();
            clearIngredientData.CloseUI();
            InventoryManager.Instance.ResetInventory();
        }
    }
}