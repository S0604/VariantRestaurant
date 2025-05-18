using UnityEngine;

public class ItemSource : MonoBehaviour
{
    public MenuItem itemToGive;

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            bool added = InventoryManager.Instance.AddItem(itemToGive);  // 改成 AddItem
            if (added)
            {
                // 可選：這個物件只能被使用一次
                // Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
