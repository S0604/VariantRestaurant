using UnityEngine;
using System.Collections.Generic;

public class CustomerOrder : MonoBehaviour
{
    public List<MenuItem> selectedItems = new List<MenuItem>();
    public bool IsOrderReady { get; private set; } = false;

    public void GenerateOrder(MenuDatabase database, int minItems = 1, int maxItems = 2)
    {
        selectedItems.Clear();
        IsOrderReady = false;

        if (database == null || database.allMenuItems.Length == 0)
        {
            Debug.LogWarning("MenuDatabase 為空或未設定！");
            return;
        }

        int count = Random.Range(minItems, maxItems + 1);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, database.allMenuItems.Length);
            selectedItems.Add(database.allMenuItems[index]); // ✅ 允許重複
        }

        IsOrderReady = true;
    }
}
