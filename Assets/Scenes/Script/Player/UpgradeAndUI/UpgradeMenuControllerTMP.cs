using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeMenuControllerTMP : MonoBehaviour
{
    [System.Serializable]
    public class Page
    {
        [Header("頁面命名與根物件")]
        public string pageName = "Supply";
        public GameObject root;            // 這個頁面的整個面板（顯示/隱藏用）

        [Header("本頁內容生成容器（不需要 ScrollView）")]
        public Transform listContainer;    // 生成 UpgradeRowUI_TMP 的父物件

        [Header("資料來源（二選一）")]
        public bool useExplicitDefs = false;
        public List<UpgradeDefinition> explicitDefs = new List<UpgradeDefinition>();

        [Tooltip("若不使用 explicitDefs，則用分類自動過濾")]
        public UpgradeCategory[] categories;

        [Header("可選：Tab 按鈕（用來切頁/高亮）")]
        public Button tabButton;

        [HideInInspector] public bool builtOnce = false;
    }

    [Header("全域")]
    [SerializeField] private GameObject panelRoot;         // 整個升級主面板
    [SerializeField] private TMP_Text coinsText;           // 顯示金錢

    [Header("鍵盤熱鍵")]
    [SerializeField] private bool enableKeyToggle = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private bool pauseOnOpen = true;

    [Header("UI 按鈕（可選，不設也能用公開方法）")]
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button toggleButton;

    [Header("列 Prefab")]
    [SerializeField] private UpgradeRowUI_TMP rowPrefab;   // 單列 Prefab（根上要掛 UpgradeRowUI_TMP）

    [Header("分頁")]
    [SerializeField] private List<Page> pages = new List<Page>();
    [SerializeField] private int defaultPageIndex = 0;     // 開啟時預設顯示哪一頁

    int currentIndex = -1;

    void Start()
    {
        // 綁 Tab 按鈕事件 + 頁面預設關閉
        for (int i = 0; i < pages.Count; i++)
        {
            int idx = i;
            if (pages[i].tabButton)
            {
                pages[i].tabButton.onClick.RemoveAllListeners();
                pages[i].tabButton.onClick.AddListener(() => ShowPageIndex(idx));
            }
            if (pages[i].root) pages[i].root.SetActive(false);
        }

        // 綁「外層」開關按鈕（若有指定）
        if (openButton)
        {
            openButton.onClick.RemoveAllListeners();
            openButton.onClick.AddListener(() => Open(defaultPageIndex));
        }
        if (closeButton)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }
        if (toggleButton)
        {
            toggleButton.onClick.RemoveAllListeners();
            toggleButton.onClick.AddListener(() => Toggle(defaultPageIndex));
        }

        if (panelRoot) panelRoot.SetActive(false);

        if (PlayerData.Instance)
            PlayerData.Instance.OnStatsChanged += RefreshCoins;

        RefreshCoins();
    }

    void OnDestroy()
    {
        if (PlayerData.Instance)
            PlayerData.Instance.OnStatsChanged -= RefreshCoins;
    }

    void Update()
    {
        if (!enableKeyToggle) return;
        if (Input.GetKeyDown(toggleKey))
            Toggle(defaultPageIndex);
    }

    // --------------------
    //  公開方法（給 UI OnClick 直接用）
    // --------------------
    public void Open() => Open(defaultPageIndex);
    public void Open(int pageIndex)
    {
        if (!panelRoot) return;
        panelRoot.SetActive(true);
        if (pauseOnOpen) Time.timeScale = 0f;

        if (pages.Count > 0)
            ShowPageIndex(Mathf.Clamp(pageIndex, 0, pages.Count - 1));
        RefreshCoins();
    }

    public void OpenAndShowPageByName(string pageName)
    {
        if (!panelRoot) return;
        panelRoot.SetActive(true);
        if (pauseOnOpen) Time.timeScale = 0f;

        ShowPageByName(pageName);
        RefreshCoins();
    }

    public void Close()
    {
        if (!panelRoot) return;
        panelRoot.SetActive(false);
        if (pauseOnOpen) Time.timeScale = 1f;
    }

    public void Toggle() => Toggle(defaultPageIndex);
    public void Toggle(int pageIndex)
    {
        if (!panelRoot) return;
        bool next = !panelRoot.activeSelf;
        if (next) Open(pageIndex);
        else Close();
    }

    public void ShowPageByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return;
        string target = name.Trim();
        int idx = pages.FindIndex(p =>
            string.Equals(p.pageName?.Trim(), target, System.StringComparison.OrdinalIgnoreCase));
        if (idx >= 0) ShowPageIndex(idx);
        else Debug.LogWarning($"[UpgradeMenu] Page '{name}' not found.");
    }


    public void ShowPageIndex(int index)
    {
        if (index < 0 || index >= pages.Count) return;

        // 切換可見
        for (int i = 0; i < pages.Count; i++)
        {
            bool active = (i == index);
            if (pages[i].root) pages[i].root.SetActive(active);

            // Tab 高亮（可選）
            if (pages[i].tabButton)
                pages[i].tabButton.interactable = !active;
        }

        currentIndex = index;

        // 首次進入該頁才建（要每次重建可改成永遠 BuildPage(index)）
        if (!pages[index].builtOnce)
        {
            BuildPage(index);
            pages[index].builtOnce = true;
        }

        RefreshCoins();
    }

    void BuildPage(int index)
    {
        var page = pages[index];
        if (!rowPrefab || !page.listContainer) return;

        // 清空舊列
        for (int i = page.listContainer.childCount - 1; i >= 0; i--)
            Destroy(page.listContainer.GetChild(i).gameObject);

        // 取得要顯示的定義
        List<UpgradeDefinition> defs = new List<UpgradeDefinition>();
        if (page.useExplicitDefs)
        {
            foreach (var d in page.explicitDefs)
                if (d) defs.Add(d);
        }
        else
        {
            var upg = UpgradeManager.Instance;
            if (upg)
            {
                foreach (var d in upg.upgradeDefs)
                {
                    if (!d) continue;
                    if (ArrayContains(page.categories, d.category))
                        defs.Add(d);
                }
            }
        }

        // 生成列
        foreach (var def in defs)
        {
            var row = Instantiate(rowPrefab, page.listContainer);
            row.Setup(def, UpgradeManager.Instance, null);
        }
    }

    bool ArrayContains<T>(T[] arr, T value)
    {
        if (arr == null || arr.Length == 0) return false;
        foreach (var a in arr) if (Equals(a, value)) return true;
        return false;
    }

    void RefreshCoins()
    {
        if (!coinsText || PlayerData.Instance == null) return;
        coinsText.text = $"$ {PlayerData.Instance.stats.money}";
    }
}
