using System.Collections.Generic;
using UnityEngine;

public class CustomerQueueManager : MonoBehaviour
{
    public static CustomerQueueManager Instance;

    public List<Transform> queuePathPoints;
    public float queueSpacing = 1.5f;
    public int maxQueueSize = 5;

    private Customer[] queueSlots;

    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
        queueSlots = new Customer[maxQueueSize];
    }

    #region 進出隊列
    public int FindFirstAvailableSlot() => System.Array.FindIndex(queueSlots, s => s == null);
    public bool AssignCustomerToSlot(Customer customer, int idx)
    {
        if (idx < 0 || idx >= queueSlots.Length || queueSlots[idx] != null) return false;
        queueSlots[idx] = customer; UpdateQueuePositions(); return true;
    }
    public void LeaveQueue(Customer customer)
    {
        for (int i = 0; i < queueSlots.Length; i++)
        {
            if (queueSlots[i] == customer)
            { queueSlots[i] = null; ShiftQueueForward(); UpdateQueuePositions(); break; }
        }
    }
    #endregion

    #region 內部運算
    void ShiftQueueForward()
    {
        for (int i = 0; i < queueSlots.Length - 1; i++)
            if (queueSlots[i] == null) { queueSlots[i] = queueSlots[i + 1]; queueSlots[i + 1] = null; }
    }
    void UpdateQueuePositions()
    {
        for (int i = 0; i < queueSlots.Length; i++)
            if (queueSlots[i] != null)
            {
                GetPosDir(i, out Vector3 pos, out Vector3 dir);
                queueSlots[i].SetQueuePosition(pos, dir);
            }
    }
    void GetPosDir(int index, out Vector3 pos, out Vector3 dir)
    {
        float dist = index * queueSpacing;
        for (int i = 0; i < queuePathPoints.Count - 1; i++)
        {
            var a = queuePathPoints[i].position; var b = queuePathPoints[i + 1].position;
            float seg = Vector3.Distance(a, b);
            if (dist <= seg)
            { dir = (a - b).normalized; pos = a + (b - a).normalized * dist; return; }
            dist -= seg;
        }
        pos = queuePathPoints[^1].position;
        dir = (queuePathPoints[^2].position - pos).normalized;
    }
    #endregion

    #region 公開查詢/強制操作
    public List<Customer> GetCurrentQueue()
    {
        var list = new List<Customer>();
        foreach (var c in queueSlots) if (c != null) list.Add(c);
        return list;
    }
    public bool IsInQueue(Customer c) => System.Array.Exists(queueSlots, s => s == c);
    public int GetCustomerPosition(Customer c) => GetCurrentQueue().IndexOf(c);

    public void ForceRemoveCustomersAt15F()
    {
        var q = GetCurrentQueue();
        for (int i = 4; i < q.Count; i++) q[i].LeaveAndDespawn();

        foreach (var c in ModeToggleManager.Instance.GetAliveCustomers())
            if (!IsInQueue(c)) c.LeaveAndDespawn();
    }
    public void ForceRemoveAllCustomers()
    {
        foreach (var c in ModeToggleManager.Instance.GetAliveCustomers()) c.LeaveAndDespawn();
    }
    #endregion
}
