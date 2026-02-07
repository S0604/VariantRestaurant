using UnityEngine;
using UnityEngine.UI;

public class CookingStation : MonoBehaviour
{
    private bool playerInRange = false;

    [Tooltip("這個站點的小遊戲類型，如 Burger、Fries、Drink")]
    public string minigameType = "Burger";

    [Header("能量條設定")]
    [Tooltip("Inspector 預設值；若有 WorkbenchMaxEnergy 定義，會以升級值覆蓋")]
    public int maxEnergy = 3;

    [SerializeField] private int currentEnergy;

    public Image energyMask;
    public GameObject energyBarUI;

    public MenuItem energySupplyItem;

    [Header("UI 引用")]
    public Transform supplyContainer;

    [Header("升級套用")]
    public bool useUpgradeMaxEnergy = true;

    [Tooltip("WorkbenchMaxEnergy 變更時是否回滿能量")]
    public bool refillEnergyWhenMaxEnergyChanges = true;

    private void OnEnable()
    {
        UpgradeManager.OnAnyLevelChanged += HandleAnyUpgradeChanged;
    }

    private void OnDisable()
    {
        UpgradeManager.OnAnyLevelChanged -= HandleAnyUpgradeChanged;
    }

    private void Start()
    {
        ApplyMaxEnergyFromUpgrade(refillToFull: true);
        if (currentEnergy <= 0) currentEnergy = maxEnergy;
        UpdateEnergyUI();
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null) return;

        if (HasSupplyItem(inventory))
        {
            if (currentEnergy < maxEnergy)
            {
                RemoveSupplyItem(inventory);

                int add = GetSupplyAmount();
                currentEnergy = Mathf.Min(currentEnergy + add, maxEnergy);

                UpdateEnergyUI();
                ClearSupplyUI();
                Debug.Log($"成功補充能量 +{add}（{currentEnergy}/{maxEnergy}）");
            }
            else
            {
                Debug.Log("能量已滿，無需補給");
            }
            return;
        }

        if (currentEnergy <= 0)
        {
            Debug.Log("能量不足，無法開始小遊戲");
            return;
        }

        if (MinigameManager.Instance != null && MinigameManager.Instance.IsPlaying)
        {
            Debug.Log("已有小遊戲正在進行");
            return;
        }

        Debug.Log("開始小遊戲: " + minigameType);
        MinigameManager.Instance.StartMinigame(minigameType, OnMinigameComplete);
    }

    private bool HasSupplyItem(InventoryManager inventory)
    {
        if (inventory == null) return false;
        if (energySupplyItem == null) return false;

        string supplyTag = energySupplyItem.itemTag;
        if (string.IsNullOrEmpty(supplyTag)) return false;

        foreach (var item in inventory.GetItems())
        {
            if (item != null && item.itemTag == supplyTag)
                return true;
        }
        return false;
    }

    private void RemoveSupplyItem(InventoryManager inventory)
    {
        if (inventory == null || energySupplyItem == null) return;

        string supplyTag = energySupplyItem.itemTag;
        var items = inventory.GetItems();

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null && items[i].itemTag == supplyTag)
            {
                inventory.RemoveItem(items[i]);
                break;
            }
        }
    }

    private void OnMinigameComplete(bool success, int rank)
    {
        currentEnergy = Mathf.Max(currentEnergy - 1, 0);
        UpdateEnergyUI();
    }

    private void UpdateEnergyUI()
    {
        if (maxEnergy <= 0) maxEnergy = 1;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);

        float ratio = (float)currentEnergy / maxEnergy;
        if (energyMask != null)
            energyMask.fillAmount = ratio;

        if (energyBarUI != null)
            energyBarUI.SetActive(true);
    }

    private int GetSupplyAmount()
    {
        var upg = UpgradeManager.Instance;
        if (upg == null) return 1;

        float v = upg.GetValue(UpgradeType.SupplyPickupAmount);
        int n = Mathf.RoundToInt(v);
        return Mathf.Max(1, n);
    }

    private void ApplyMaxEnergyFromUpgrade(bool refillToFull)
    {
        if (!useUpgradeMaxEnergy) return;

        int oldMax = Mathf.Max(1, maxEnergy);

        var upg = UpgradeManager.Instance;
        if (upg == null)
        {
            maxEnergy = oldMax;
            currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
            return;
        }

        float v = upg.GetValue(UpgradeType.WorkbenchMaxEnergy);
        int newMax = Mathf.RoundToInt(v);
        if (newMax <= 0) newMax = oldMax;

        maxEnergy = Mathf.Max(1, newMax);

        if (refillToFull)
            currentEnergy = maxEnergy;
        else
            currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
    }

    private void HandleAnyUpgradeChanged(UpgradeType type, int newLevel)
    {
        if (type != UpgradeType.WorkbenchMaxEnergy) return;

        ApplyMaxEnergyFromUpgrade(refillEnergyWhenMaxEnergyChanges);
        UpdateEnergyUI();
    }

    private void ClearSupplyUI()
    {
        if (!supplyContainer) return;
        for (int i = supplyContainer.childCount - 1; i >= 0; i--)
            Destroy(supplyContainer.GetChild(i).gameObject);
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
