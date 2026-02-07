using UnityEngine;

public class FreeCustomerSpawner : MonoBehaviour
{
    [Header("生成設定")]
    public GameObject[] customerPrefabs;
    public Transform[] spawnPoints;

    [Header("營業模式來源")]
    public FreeModeToggleManager modeManager;

    private void Start()
    {
        if (modeManager == null)
            modeManager = FreeModeToggleManager.Instance;
    }

    public static FreeCustomerSpawner Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }    
    // 需要跨場景就開啟
    /// <summary>生成一位顧客（由外部呼叫）</summary>
    public void SpawnCustomer(int index = -1)
    {
        if (modeManager == null || !modeManager.IsBusinessMode)
        {
            Debug.LogWarning("⚠️ 無法生成：目前不是營業模式或 modeManager 未設定！");
            return;
        }

        if (customerPrefabs.Length == 0 || spawnPoints.Length == 0)
        {
            Debug.LogWarning("❌ FreeCustomerSpawner 缺少 Prefab 或 SpawnPoints 設定！");
            return;
        }

        /* 1. 依列表順序（index ≥ 0）→ 照順序 0→1→2→... */
        if (index >= 0 && index < customerPrefabs.Length && index < spawnPoints.Length)
        {
            Transform spawnPoint = spawnPoints[index];
            GameObject prefab = customerPrefabs[index];
            GameObject newCustomer = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            Customer customerScript = newCustomer.GetComponent<Customer>();
            if (customerScript != null)
                customerScript.spawnPoint = spawnPoint;

            Debug.Log($"✅ 依序生成顧客（序號 {index}）：{newCustomer.name} 於 {spawnPoint.name}");
            return;
        }

        /* 2. 沒給 index → 隨機 */
        int prefabIndex = Random.Range(0, customerPrefabs.Length);
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        Transform randomSpawn = spawnPoints[spawnIndex];
        GameObject randomPrefab = customerPrefabs[prefabIndex];
        GameObject randomCustomer = Instantiate(randomPrefab, randomSpawn.position, Quaternion.identity);
        Customer randomScript = randomCustomer.GetComponent<Customer>();
        if (randomScript != null)
            randomScript.spawnPoint = randomSpawn;

        Debug.Log($"✅ 隨機生成顧客：{randomCustomer.name} 於 {randomSpawn.name}");
    }

    /// <summary>依列表順序生成多名顧客</summary>
    public void SpawnCustomersInOrder(int count)
    {
        count = Mathf.Min(count, customerPrefabs.Length, spawnPoints.Length);
        for (int i = 0; i < count; i++)
        {
            SpawnCustomer(i);   // 強制依序 0→1→2→...
        }
    }
}