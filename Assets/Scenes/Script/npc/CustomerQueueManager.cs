using UnityEngine;
using System.Collections.Generic;

public class CustomerQueueManager : MonoBehaviour
{
    public static CustomerQueueManager Instance { get; private set; }

    public List<Transform> queuePositions;

    // ✅ 關鍵宣告：確保這行存在
    private List<Customer> customers = new List<Customer>();

    private void Awake()
    {
        Instance = this;
    }

    public void JoinQueue(Customer customer)
    {
        customers.Add(customer);
        UpdateQueuePositions();
    }

    public void LeaveQueue(Customer customer)
    {
        customers.Remove(customer);
        UpdateQueuePositions();
    }

    private void UpdateQueuePositions()
    {
        for (int i = 0; i < customers.Count; i++)
        {
            if (i < queuePositions.Count)
            {
                Vector3 pos = queuePositions[i].position;
                Vector3 dir = queuePositions[i].forward;
                customers[i].SetQueuePosition(pos, dir);
            }
            else
            {
                Debug.LogWarning($"【QueueManager】沒有足夠的排隊位置給第 {i} 位顧客");
            }
        }
    }
}
