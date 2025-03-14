using UnityEngine;

public class CookingMenuUI_Extended : CookingMenuUI
{
    public GameObject SelectionUI; // ?? UI
    public GameObject NextStepUI;  // ??? UI

    public void CheckIngredientsAndToggleUI()
    {
        if (itemSlots == null || itemSlots.Length < 4)
        {
            Debug.LogError("? itemSlots ???????");
            return;
        }

        bool hasBase = itemSlots[3].GetIngredientData() != null; // ???
        bool hasIngredient = itemSlots[0].GetIngredientData() != null ||
                             itemSlots[1].GetIngredientData() != null ||
                             itemSlots[2].GetIngredientData() != null; // ??????

        if (hasBase && hasIngredient)
        {
            SelectionUI.SetActive(false);
            NextStepUI.SetActive(true);
        }
        else
        {
            SelectionUI.SetActive(true);
            NextStepUI.SetActive(false);
        }
    }
}
