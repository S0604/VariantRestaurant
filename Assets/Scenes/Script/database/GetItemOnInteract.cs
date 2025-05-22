using UnityEngine;

public class GetItemOnInteract : MonoBehaviour
{
    public MenuItem menuItem; // 要給玩家的物品
    public Inventory playerInventory; // 玩家物品欄
    public float interactRange = 3f; // 互動距離

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerInventory == null)
            Debug.LogError("請在Inspector設定playerInventory參考");
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= interactRange && Input.GetKeyDown(KeyCode.E))
        {
            if (playerInventory.items.Count < playerInventory.maxSlots)
            {
                playerInventory.AddItem(menuItem);
                Debug.Log($"獲得物品：{menuItem.itemName}");
                // 不再銷毀物件，Cube會一直存在，可以多次互動
            }
            else
            {
                Debug.Log("物品欄已滿，無法獲得物品");
            }
        }
    }
}