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

    [Header("教學開關")]
    //public bool allowStart = false;   // 由 FeatureLockerPro 控制
    public Image energyMask;
    public GameObject energyBarUI;

    public MenuItem energySupplyItem; // 指定補給箱 MenuItem

    private bool instantCookEnabled = false;
    private float instantCookEndTime = 0f;

    public void EnableInstantCook(float duration)
    {
        instantCookEnabled = true;
        instantCookEndTime = Time.time + duration;
    }

    void Start()
    {
        {
            currentEnergy = maxEnergy;

            // 套用被動技能加成（若有）
            if (PassiveSkillManager.Instance != null && PassiveSkillManager.Instance.maxEnergyBonus > 0)
            {
                maxEnergy += PassiveSkillManager.Instance.maxEnergyBonus;
                currentEnergy = maxEnergy;
            }

            UpdateEnergyUI();
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        //if (!allowStart)
        {
            //Debug.Log("料理功能尚未解鎖！");
           // return;
        }

        if (instantCookEnabled && Time.time < instantCookEndTime)
        {
            Debug.Log("InstantCookSkill 效果中，直接完成料理！");

            //  自動生成對應料理 MenuItem
            MenuItem cookedDish = MenuDatabase.Instance.GetMenuItemByTag(minigameType);
            if (cookedDish != null)
            {
                // 複製一份新物件（避免直接修改原 ScriptableObject）
                MenuItem newItem = ScriptableObject.Instantiate(cookedDish);
                newItem.grade = BaseMinigame.DishGrade.Perfect; // 直接設定為最高評級
                newItem.SyncImageToGrade();

                //  加入玩家物品欄
                InventoryManager.Instance.AddItem(newItem);

                Debug.Log($"🍽 已獲得料理：{newItem.itemName}（Perfect）");
            }
            else
            {
                Debug.LogWarning($"❌ 無法找到料理資料：{minigameType}");
            }

            // 減少能量與更新 UI
            currentEnergy = Mathf.Max(currentEnergy - 1, 0);
            UpdateEnergyUI();

            // ✅ 不啟動小遊戲
            return;
        }

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
        // ✅ 如果 Buff 啟動中 → 無論成功或失敗都會在這次觸發後清除
        bool hadPerfectBuff = PerfectCookBuffManager.Instance != null && PerfectCookBuffManager.Instance.IsBuffActive();

        if (!success)
        {
            Debug.Log($"{minigameType} 製作失敗 ❌");
            if (hadPerfectBuff)
            {
                Debug.Log("🌀 PerfectCookBuff 因製作失敗而消失。");
                PerfectCookBuffManager.Instance.ConsumeBuff();
            }
            return;
        }

        Debug.Log($"{minigameType} 製作成功，等級: {rank}");

        if (hadPerfectBuff)
        {
            if (rank > 1) // 只影響 Good 以上
            {
                Debug.Log("🎯 PerfectCookBuff 生效 → 評級強制變為 Perfect！");
                rank = 3;
            }
            else
            {
                Debug.Log("💤 PerfectCookBuff 無效（評級太低），但依然被消耗。");
            }

            PerfectCookBuffManager.Instance.ConsumeBuff();
        }

        // ⚙️ 原本的能量與 UI 更新
        currentEnergy = Mathf.Max(currentEnergy - 1, 0);
        UpdateEnergyUI();

        Debug.Log($"{minigameType} 最終評級為：{rank}");
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
        return UpgradeManager.Instance != null ? UpgradeManager.Instance.supplyAmount : 1;
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
