using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpgradeMenuControllerTMP : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private GameObject panelRoot;      // 整個面板
    [SerializeField] private Transform listContainer;   // 內容父物件（Vertical Layout Content）
    [SerializeField] private UpgradeRowUI_TMP rowPrefab;// 單列 Prefab
    [SerializeField] private TMP_Text coinsText;        // 金錢文本（可用 PlayerData.money）

    [Header("Open/Close")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private bool pauseOnOpen = true;

    private readonly List<UpgradeRowUI_TMP> rows = new();

    void Start()
    {
        BuildRows();
        RefreshCoins();

        if (panelRoot) panelRoot.SetActive(false);
        if (PlayerData.Instance) PlayerData.Instance.OnStatsChanged += RefreshCoins;
    }

    void OnDestroy()
    {
        if (PlayerData.Instance) PlayerData.Instance.OnStatsChanged -= RefreshCoins;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) Toggle();
    }

    public void Toggle()
    {
        if (!panelRoot) return;
        bool next = !panelRoot.activeSelf;
        panelRoot.SetActive(next);
        if (pauseOnOpen) Time.timeScale = next ? 0f : 1f;
    }

    void BuildRows()
    {
        var upg = UpgradeManager.Instance;
        if (!upg || !rowPrefab || !listContainer) return;

        for (int i = listContainer.childCount - 1; i >= 0; i--)
            Destroy(listContainer.GetChild(i).gameObject);
        rows.Clear();

        foreach (var def in upg.upgradeDefs)
        {
            if (!def) continue;
            var row = Instantiate(rowPrefab, listContainer);
            row.Setup(def, upg, this);
            rows.Add(row);
        }
    }

    void RefreshCoins()
    {
        if (!coinsText || PlayerData.Instance == null) return;
        coinsText.text = $"$ {PlayerData.Instance.stats.money}";
        // 同步各列按鈕可否點
        foreach (var r in rows) r.RefreshInteractable();
    }
}
