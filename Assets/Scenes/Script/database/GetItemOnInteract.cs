using UnityEngine;

public class GetItemOnInteract : MonoBehaviour
{
    public MenuItem menuItem;
    public Inventory playerInventory;
    private Transform player;
    private bool isPlayerInTrigger = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerInventory == null)
            Debug.LogError("請在Inspector設定playerInventory參考");
    }

    void Update()
    {
        if (player == null || !isPlayerInTrigger) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (playerInventory.items.Count < playerInventory.maxSlots)
            {
                playerInventory.AddItem(menuItem);
                Debug.Log($"獲得物品：{menuItem.itemName}");
            }
            else
            {
                Debug.Log("物品欄已滿，無法獲得物品");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
        }
    }
}
