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

        int prefabIndex = (index >= 0 && index < customerPrefabs.Length)
            ? index
            : Random.Range(0, customerPrefabs.Length);

        int spawnIndex = Mathf.Min(index, spawnPoints.Length - 1);
        Transform spawnPoint = spawnPoints[spawnIndex];
        GameObject prefab = customerPrefabs[prefabIndex];

        GameObject newCustomer = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        Customer customerScript = newCustomer.GetComponent<Customer>();

        if (customerScript != null)
            customerScript.spawnPoint = spawnPoint;

        Debug.Log($"✅ 已生成顧客（序號 {index}）：{newCustomer.name} 於 {spawnPoint.name}");
    }

    /// <summary>依序生成多名顧客</summary>
    public void SpawnCustomersInOrder(int count)
    {
        if (count > spawnPoints.Length)
            count = spawnPoints.Length;

        for (int i = 0; i < count; i++)
        {
            SpawnCustomer(i);
        }
    }
}
