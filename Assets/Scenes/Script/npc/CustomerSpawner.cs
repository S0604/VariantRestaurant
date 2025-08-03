using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("生成設定")]
    public GameObject[] customerPrefabs;
    public GameObject[] specialCustomerPrefabs; // ✅ 多樣式特殊顧客
    public Transform[] spawnPoints;

    [Header("節奏設定")]
    public float maxInterval = 8f;
    public float minInterval = 2f;
    [Range(0f, 1f)]
    public float specialCustomerChance = 0.1f; // ✅ 特殊顧客出現機率

    [Header("營業時間來源")]
    public ModeToggleManager modeManager;

    private float timer;
    private float totalGameDuration;
    private float gameTime;

    void OnEnable()
    {
        timer = 0f;
        gameTime = 0f;

        if (modeManager != null)
        {
            totalGameDuration = modeManager.businessDuration;
        }
        else
        {
            totalGameDuration = 180f;
            Debug.LogWarning("未指定 ModeToggleManager，使用預設時間");
        }
    }

    void Update()
    {
        if (modeManager == null) return;

        // 🛑 關店準備階段不再生成顧客
        if (modeManager.RemainingBusinessTime > 10f)
        {
            timer += Time.deltaTime;
            gameTime += Time.deltaTime;

            float currentInterval = GetCurrentSpawnInterval();
            if (timer >= currentInterval)
            {
                SpawnCustomer();
                timer = 0f;
            }
        }
    }

    float GetCurrentSpawnInterval()
    {
        float t = Mathf.Clamp01(gameTime / totalGameDuration);
        float peakCurve = -4 * Mathf.Pow(t - 0.5f, 2) + 1;
        float baseInterval = Mathf.Lerp(maxInterval, minInterval, peakCurve);
        return Mathf.Lerp(maxInterval, minInterval, peakCurve);

    }

    void SpawnCustomer()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("缺少生成點！");
            return;
        }

        Transform chosenSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        bool specialCustomerExists = GameObject.FindWithTag("SpecialCustomer") != null;

        GameObject prefabToSpawn;

        if (!specialCustomerExists && specialCustomerPrefabs.Length > 0 && Random.value < specialCustomerChance)
        {
            int specialIndex = Random.Range(0, specialCustomerPrefabs.Length);
            prefabToSpawn = specialCustomerPrefabs[specialIndex];
        }
        else
        {
            if (customerPrefabs.Length == 0)
            {
                Debug.LogWarning("缺少一般顧客Prefab！");
                return;
            }

            int normalIndex = Random.Range(0, customerPrefabs.Length);
            prefabToSpawn = customerPrefabs[normalIndex];
        }

        GameObject customer = Instantiate(prefabToSpawn, chosenSpawnPoint.position, Quaternion.identity);

        Customer customerScript = customer.GetComponent<Customer>();
        if (customerScript != null)
        {
            customerScript.spawnPoint = chosenSpawnPoint;
        }

        // ❌ 不再觸發特殊顧客效果，交由觸發區域判斷
    }
}
