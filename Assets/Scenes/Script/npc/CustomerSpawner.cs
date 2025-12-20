using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("生成設定")]
    public GameObject[] customerPrefabs;            // 一般顧客
    public GameObject[] specialCustomerPrefabs;     // 特殊顧客
    public Transform[] spawnPoints;

    [Header("節奏設定")]
    public float maxInterval = 8f;
    public float minInterval = 2f;
    [Range(0f, 1f)]
    public float specialCustomerChance = 0.1f;

    [Header("營業時間來源")]
    public FreeModeToggleManager modeManager;

    [Header("顧客數量設定")]
    public int baseMaxCustomers = 5;             // 初始顧客上限
    public int maxCustomers = 5;                 // 動態顧客上限
    public int maxIncreaseByPopularity = 13;     // 人氣能額外提升的最大值（搭配分段式使用可忽略）

    private float timer;
    private float totalGameDuration;
    private float gameTime;

    //------------------------------
    // 啟動 / 關閉事件訂閱
    //------------------------------
    private void OnEnable()
    {
        timer = 0f;
        gameTime = 0f;

        if (modeManager != null)
            totalGameDuration = modeManager.businessDuration;
        else
        {
            totalGameDuration = 180f;
            Debug.LogWarning("未指定 FreeModeToggleManager，使用預設時間 180 秒");
        }

        // 訂閱 PlayerData 的數值變動事件
        if (PlayerData.Instance != null)
            PlayerData.Instance.OnStatsChanged += HandleStatsChanged;

        // 初始化顧客上限
        UpdateMaxCustomers(PlayerData.Instance.stats.popularity);
    }

    private void OnDisable()
    {
        if (PlayerData.Instance != null)
            PlayerData.Instance.OnStatsChanged -= HandleStatsChanged;
    }

    //------------------------------
    // 當 PlayerData stats 有變化
    //------------------------------
    private void HandleStatsChanged()
    {
        UpdateMaxCustomers(PlayerData.Instance.stats.popularity);
    }

    //------------------------------
    // 分段式：人氣 → 顧客上限
    //------------------------------
    public void UpdateMaxCustomers(int popularity)
    {
        if (popularity <= 20)
            maxCustomers = 5;
        else if (popularity <= 50)
            maxCustomers = 7;
        else if (popularity <= 80)
            maxCustomers = 9;
        else if (popularity <= 120)
            maxCustomers = 12;
        else if (popularity <= 200)
            maxCustomers = 15;
        else
            maxCustomers = 18; // 封頂

        Debug.Log($"📢 人氣 = {popularity} → maxCustomers = {maxCustomers}");
    }

    //------------------------------
    // Update：照節奏生成顧客
    //------------------------------
    private void Update()
    {
        if (modeManager == null) return;

        if (!modeManager.AllowTimeFlow || !modeManager.IsBusinessMode)
            return;

        // 關店前不再生成顧客
        if (modeManager.RemainingBusinessTime <= modeManager.closingBufferTime)
            return;

        timer += Time.deltaTime;
        gameTime += Time.deltaTime;

        float currentInterval = GetCurrentSpawnInterval();
        if (timer >= currentInterval)
        {
            SpawnCustomer();
            timer = 0f;
        }
    }

    //------------------------------
    // 曲線節奏（你原本的）
    //------------------------------
    private float GetCurrentSpawnInterval()
    {
        float t = Mathf.Clamp01(gameTime / totalGameDuration);
        float peakCurve = -4 * Mathf.Pow(t - 0.5f, 2) + 1; // 鐘型曲線
        return Mathf.Lerp(maxInterval, minInterval, peakCurve);
    }

    //------------------------------
    // 實際生成顧客
    //------------------------------
    private void SpawnCustomer()
    {
        // 🛑 顧客上限檢查
        int currentCount = FindObjectsOfType<Customer>().Length;
        if (currentCount >= maxCustomers)
            return;

        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("⚠ 缺少生成點！");
            return;
        }

        Transform chosenSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject prefabToSpawn;

        bool specialExists = GameObject.FindWithTag("SpecialCustomer") != null;

        // 特殊顧客生成條件
        if (!specialExists &&
            specialCustomerPrefabs.Length > 0 &&
            Random.value < specialCustomerChance)
        {
            prefabToSpawn = specialCustomerPrefabs[Random.Range(0, specialCustomerPrefabs.Length)];
        }
        else
        {
            if (customerPrefabs.Length == 0)
            {
                Debug.LogWarning("⚠ 缺少一般顧客 Prefab！");
                return;
            }

            prefabToSpawn = customerPrefabs[Random.Range(0, customerPrefabs.Length)];
        }

        // 實際生成
        GameObject customer = Instantiate(prefabToSpawn, chosenSpawnPoint.position, Quaternion.identity);

        // 指定來源點
        Customer customerScript = customer.GetComponent<Customer>();
        if (customerScript != null)
            customerScript.spawnPoint = chosenSpawnPoint;
    }
}
