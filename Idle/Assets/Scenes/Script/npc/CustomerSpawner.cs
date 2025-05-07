using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("生成設定")]
    public GameObject[] customerPrefabs; // 可用的顧客Prefab
    public Transform spawnPoint;         // 生成位置
    public float spawnInterval = 5f;      // 生成間隔（秒）

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnCustomer();
            timer = 0f;
        }
    }

    void SpawnCustomer()
    {
        if (customerPrefabs.Length == 0)
        {
            Debug.LogWarning("沒有設定顧客Prefab！");
            return;
        }

        // 隨機選一個顧客Prefab
        int index = Random.Range(0, customerPrefabs.Length);
        GameObject customer = Instantiate(customerPrefabs[index], spawnPoint.position, Quaternion.identity);

        Customer customerScript = customer.GetComponent<Customer>();
        if (customerScript != null)
        {
            customerScript.spawnPoint = spawnPoint;
        }
    }
}
