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

    /// <summary>
    /// 生成一位顧客（由外部手動呼叫）
    /// </summary>
    public void SpawnCustomer()
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

        // 隨機選擇生成點與顧客預置物
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject prefab = customerPrefabs[Random.Range(0, customerPrefabs.Length)];

        GameObject newCustomer = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        Customer customerScript = newCustomer.GetComponent<Customer>();

        if (customerScript != null)
        {
            customerScript.spawnPoint = spawnPoint;
        }

        Debug.Log($"✅ 已生成顧客：{newCustomer.name} 於 {spawnPoint.name}");
    }
}
