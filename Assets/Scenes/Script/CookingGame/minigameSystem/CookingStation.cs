using UnityEngine;
using UnityEngine.UI;

public class CookingStation : MonoBehaviour
{
    private bool playerInRange = false;
    private static bool firstEnergyDepleted = false;

    [Tooltip("這個站點的小遊戲類型，如 Burger、Fries、Drink")]
    public string minigameType = "Burger";

    [Header("能量條設定")]
    [Tooltip("Inspector 預設值；若有 WorkbenchMaxEnergy 升級會覆蓋")]
    public int maxEnergy = 3;

    [Header("Highlight")]
    [SerializeField] private StationHighlighter_SwapOutlineMat highlighter;

    [SerializeField] private int currentEnergy;

    public Image energyMask;
    public GameObject energyBarUI;

    [Header("補給箱")]
    public MenuItem energySupplyItem;
    public Transform supplyContainer;

    [Header("升級套用")]
    public bool useUpgradeMaxEnergy = true;
    public bool refillEnergyWhenMaxEnergyChanges = true;

    private void Awake()
    {
        if (!highlighter)
            highlighter = GetComponentInChildren<StationHighlighter_SwapOutlineMat>(true);
    }


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
        // Time.timeScale = 0（對話、暫停）時禁止互動
        if (Time.timeScale <= 0f) return;

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
            TryInteract();
    }

    private void TryInteract()
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null) return;

        bool hasSupply = HasSupplyItem(inventory);

        // ===== 補給箱優先 =====
        if (hasSupply)
        {
            if (currentEnergy < maxEnergy)
            {
                RemoveSupplyItem(inventory);
                int add = GetSupplyAmount();
                currentEnergy = Mathf.Min(currentEnergy + add, maxEnergy);

                UpdateEnergyUI();
                ClearSupplyUI();
                Debug.Log($"成功補充能量 +{add}");
            }
            else
            {
                Debug.Log("能量已滿，無需補給");
            }
            return;
        }

        // ===== 無法開始條件 =====
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

        // ===== 啟動小遊戲 =====
        Debug.Log("開始小遊戲: " + minigameType);
        MinigameManager.Instance.StartMinigame(minigameType, OnMinigameComplete);
    }

    private void OnMinigameComplete(bool success, int rank)
    {
        bool wasPositive = currentEnergy > 0;
        currentEnergy = Mathf.Max(currentEnergy - 1, 0);

        // 第一次能量從 >0 → 0
        if (!firstEnergyDepleted && wasPositive && currentEnergy == 0)
        {
            firstEnergyDepleted = true;

            if (TutorialDialogueController.Instance != null)
                TutorialDialogueController.Instance.PlayChapter("14");

            if (TutorialProgressManager.Instance != null)
                TutorialProgressManager.Instance.CompleteEvent("EnergyDepleted");
        }

        Debug.Log(success
            ? $"{minigameType} 製作成功，等級: {rank}"
            : $"{minigameType} 製作失敗");

        UpdateEnergyUI();
    }

    // =======================
    // 能量 / UI
    // =======================

    private void UpdateEnergyUI()
    {
        if (maxEnergy <= 0) maxEnergy = 1;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);

        if (energyMask != null)
            energyMask.fillAmount = (float)currentEnergy / maxEnergy;

        if (energyBarUI != null)
            energyBarUI.SetActive(true);
    }

    // =======================
    // 補給箱
    // =======================

    private bool HasSupplyItem(InventoryManager inventory)
    {
        if (energySupplyItem == null) return false;

        foreach (var item in inventory.GetItems())
        {
            if (item != null && item.itemTag == energySupplyItem.itemTag)
                return true;
        }
        return false;
    }

    private void RemoveSupplyItem(InventoryManager inventory)
    {
        var items = inventory.GetItems();
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null && items[i].itemTag == energySupplyItem.itemTag)
            {
                inventory.RemoveItem(items[i]);
                break;
            }
        }
    }

    private int GetSupplyAmount()
    {
        var upg = UpgradeManager.Instance;
        if (upg == null) return 1;

        int v = Mathf.RoundToInt(upg.GetValue(UpgradeType.SupplyPickupAmount));
        return Mathf.Max(1, v);
    }

    private void ClearSupplyUI()
    {
        if (!supplyContainer) return;

        for (int i = supplyContainer.childCount - 1; i >= 0; i--)
            Destroy(supplyContainer.GetChild(i).gameObject);
    }

    // =======================
    // 升級
    // =======================

    private void ApplyMaxEnergyFromUpgrade(bool refillToFull)
    {
        if (!useUpgradeMaxEnergy) return;

        var upg = UpgradeManager.Instance;
        if (upg == null) return;

        int newMax = Mathf.RoundToInt(upg.GetValue(UpgradeType.WorkbenchMaxEnergy));
        if (newMax <= 0) return;

        maxEnergy = Mathf.Max(1, newMax);
        currentEnergy = refillToFull
            ? maxEnergy
            : Mathf.Clamp(currentEnergy, 0, maxEnergy);
    }

    private void HandleAnyUpgradeChanged(UpgradeType type, int newLevel)
    {
        if (type != UpgradeType.WorkbenchMaxEnergy) return;

        ApplyMaxEnergyFromUpgrade(refillEnergyWhenMaxEnergyChanges);
        UpdateEnergyUI();
    }

    // =======================
    // Trigger
    // =======================

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (highlighter) highlighter.SetHighlight(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (highlighter) highlighter.SetHighlight(false);
        }
    }

}
