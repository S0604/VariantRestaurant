using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CookButtonHandler : MonoBehaviour
{
    public CookingSystem cookingSystem;
    public GameObject renameUI;
    public RenameUIHandler renameUIHandler;
    public List<ItemSlot> itemSlots;
    private List<IngredientData> selectedIngredients = new List<IngredientData>();

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
            renameUI.SetActive(true);
            renameUIHandler.PrepareRename(selectedIngredients);
        }
        else
        {
            Debug.Log("舊料理，不顯示 UI");
            // 這裡可以加關閉多個 UI 的邏輯
        }
    }
}