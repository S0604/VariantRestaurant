using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("生成設定")]
    public GameObject[] customerPrefabs;
    public Transform[] spawnPoints;

    [Header("節奏設定")]
    public float maxInterval = 8f;
    public float minInterval = 2f;

    [Header("營業時間來源")]
    public ModeToggleManager modeManager; // ← 拖入 ModeToggleManager 以取得 businessDuration

    private float timer;
    private float gameTime;
    private float totalGameDuration;
    public TimeSystem timeSystem;

    void OnEnable()
    {
        timer = 0f;
        gameTime = 0f;

        if (modeManager != null && modeManager.timeSystem != null)
        {
            totalGameDuration = modeManager.timeSystem.cooldownDuration;
        }
        else
        {
            Debug.LogWarning("無法取得時間設定，使用預設180秒");
            totalGameDuration = 180f;
        }
    }

    void Update()
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

    float GetCurrentSpawnInterval()
    {
        float t = Mathf.Clamp01(gameTime / totalGameDuration);
        float peakCurve = -4 * Mathf.Pow(t - 0.5f, 2) + 1;
        return Mathf.Lerp(maxInterval, minInterval, peakCurve);
    }

    void SpawnCustomer()
    {
        if (customerPrefabs.Length == 0 || spawnPoints.Length == 0)
        {
            Debug.LogWarning("缺少顧客Prefab或生成點！");
            return;
        }

        int customerIndex = Random.Range(0, customerPrefabs.Length);
        int spawnIndex = Random.Range(0, spawnPoints.Length);

        Transform chosenSpawnPoint = spawnPoints[spawnIndex];
        GameObject customer = Instantiate(customerPrefabs[customerIndex], chosenSpawnPoint.position, Quaternion.identity);

        Customer customerScript = customer.GetComponent<Customer>();
        if (customerScript != null)
        {
            customerScript.spawnPoint = chosenSpawnPoint;
        }
    }
}
