using UnityEngine;
using UnityEngine.UI;

public class Fridge : MonoBehaviour
{
    public MenuItem supplyBoxItem;            // �ͦ����ɵ��c���~�]ScriptableObject�^
    public Transform iconSpawnPoint;          // �ɵ��ϥܥͦ���m
    public GameObject iconPrefab;             // �ϥܥΪ� Image prefab

    public int supplyAmount = 3;              // �i�Q�ɯšA��ڸɦh�֯�q
    private bool playerInRange = false;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TrySupplyBox();
        }
    }

    private void TrySupplyBox()
    {
        if (InventoryManager.Instance.GetItemCount() > 0)
        {
            Debug.Log("�I�]�������Ť~�����ɵ��c�I");
            return;
        }

        MenuItem itemInstance = Instantiate(supplyBoxItem);
        InventoryManager.Instance.ClearInventory();  // �O�I���k
        InventoryManager.Instance.AddItem(itemInstance);
        Debug.Log("�w����ɵ��c�A���ھ�ӭI�]");

        SpawnSupplyIcon(itemInstance);
    }

    private void SpawnSupplyIcon(MenuItem item)
    {
        if (iconPrefab == null || iconSpawnPoint == null || item == null)
        {
            Debug.LogWarning("Fridge �ɵ��ϥܩ|�����T�]�w");
            return;
        }

        GameObject iconObj = Instantiate(iconPrefab, iconSpawnPoint);
        Image img = iconObj.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = item.itemImage;
            img.color = Color.white;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    public void UpgradeSupplyAmount(int amount)
    {
        supplyAmount += amount;
        Debug.Log($"�B�c�ɵ��q�ɯŬ��G{supplyAmount}");
    }
}
