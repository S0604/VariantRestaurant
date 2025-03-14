using UnityEngine;
using UnityEngine.UI;

public class Switchfoodcategory : MonoBehaviour
{
    public GameObject Foodgrid;  // 食材放置格
    public GameObject Foodcategorycolumn;  // 食材分類欄UI
    public GameObject Foodcomlumn; //食材放置UI
    private Button FoodgridButton; // 食材放置格的 Button 組件

    // Start is called before the first frame update
    void Start()
    {
        FoodgridButton = Foodgrid.GetComponent<Button>();
        if (FoodgridButton != null)
        {
            FoodgridButton.onClick.AddListener(OpenFoodcategorycolumn); // 註冊點擊事件
        }
        else
        {
            Debug.LogError("Food grid does not have a Button component!");
        }
    }

    // 打開食材分類欄並隱藏食材放置格
    void OpenFoodcategorycolumn()
    {
        if (Foodcategorycolumn != null)
        {
            Foodcategorycolumn.SetActive(true);  // 顯示食材分類欄UI
        }

        if (Foodgrid != null)
        {
            Foodcomlumn.SetActive(false);  // 隱藏食材放置格
        }
    }
}
