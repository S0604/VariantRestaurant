using UnityEngine;
using UnityEngine.UI;

public class CookingStation : MonoBehaviour
{
    private bool playerInRange = false;

    [Tooltip("這個站點的小遊戲類型，如 Burger、Fries、Drink")]
    public string minigameType = "Burger";

    [Header("能量條設定")]
    public int maxEnergy = 3;
    private int currentEnergy;

    public Image energyMask;
    public GameObject energyBarUI;

    public MenuItem energySupplyItem; // 指定補給箱 MenuItem
    private static bool firstEnergyDepleted = false;
    void Start()
    {
        currentEnergy = maxEnergy;
        UpdateEnergyUI();
    }

    void Update()
    {
        /* 1. 對話期間直接 return（Time.timeScale=0 仍會跑 Update）*/
        if (Time.timeScale <= 0f) return;

        /* 2. 原有邏輯 */
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
            TryInteract();
    }
    private void TryInteract()
    {
        var inventory = InventoryManager.Instance;

        // 補給箱邏輯
        if (HasSupplyItem(inventory))
        {
            if (currentEnergy < maxEnergy)
            {
                RemoveSupplyItem(inventory);
                currentEnergy = Mathf.Min(currentEnergy + GetSupplyAmount(), maxEnergy);
                UpdateEnergyUI();
                ClearSupplyUI();
                Debug.Log("成功補充能量");
            }
            else
            {
                Debug.Log("能量已滿，無需補給");
            }
            return; // 補給後不執行其他互動
        }

        // 不能開始：能量不足或小遊戲進行中或持有補給箱
        if (currentEnergy <= 0)
        {
            Debug.Log("能量不足，無法開始小遊戲");
            return;
        }

        if (MinigameManager.Instance.IsPlaying)
        {
            Debug.Log("已有小遊戲正在進行");
            return;
        }

        if (HasSupplyItem(inventory))
        {
            Debug.Log("持有補給箱時無法開始小遊戲");
            return;
        }

        // 啟動小遊戲
        Debug.Log("開始小遊戲: " + minigameType);
        MinigameManager.Instance.StartMinigame(minigameType, OnMinigameComplete);
    }

    private bool HasSupplyItem(InventoryManager inventory)
    {
        foreach (var item in inventory.GetItems())
        {
            if (item.itemTag == energySupplyItem.itemTag)
                return true;
        }
        return false;
    }

    private void RemoveSupplyItem(InventoryManager inventory)
    {
        var items = inventory.GetItems();
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemTag == energySupplyItem.itemTag)
            {
                inventory.RemoveItem(items[i]);
                break;
            }
        }
    }

    private void OnMinigameComplete(bool success, int rank)
    {
        bool wasPositive = currentEnergy > 0;
        currentEnergy = Mathf.Max(currentEnergy - 1, 0);

        /* 第一次「從正變零」→ 播對話 1 + 解鎖 EnergyDepleted */
        if (!firstEnergyDepleted && wasPositive && currentEnergy == 0)
        {
            firstEnergyDepleted = true;

            // 1. 播對話
            if (TutorialDialogueController.Instance != null)
                TutorialDialogueController.Instance.PlayChapter("14");

            // 2. 解鎖事件
            if (TutorialProgressManager.Instance != null)
                TutorialProgressManager.Instance.CompleteEvent("EnergyDepleted");
        }

        // 原有日誌 & UI
        if (success)
            Debug.Log(minigameType + " 製作成功，等級: " + rank);
        else
            Debug.Log(minigameType + " 製作失敗");

        UpdateEnergyUI();
    }
    private void UpdateEnergyUI()
    {
        float ratio = (float)currentEnergy / maxEnergy;
        if (energyMask != null)
            energyMask.fillAmount = ratio;

        if (energyBarUI != null)
            energyBarUI.SetActive(true);
    }

    public void UpgradeEnergy(int amount)
    {
        maxEnergy += amount;
        currentEnergy = maxEnergy;
        UpdateEnergyUI();
    }

    private int GetSupplyAmount()
    {
        return UpgradeManager.Instance != null ? UpgradeManager.Instance.supplyAmount : 3;
    }

    private void ClearSupplyUI()
    {
        GameObject container = GameObject.Find("SupplyContainer");
        if (container != null)
        {
            foreach (Transform child in container.transform)
            {
                Destroy(child.gameObject);
            }
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
}
