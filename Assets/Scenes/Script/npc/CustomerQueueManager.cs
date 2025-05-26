using System.Collections.Generic;
using UnityEngine;

public class CustomerQueueManager : MonoBehaviour
{
    public static CustomerQueueManager Instance;

    public List<Transform> queuePathPoints;
    public float queueSpacing = 1.5f;
    public int maxQueueSize = 5;

    private Customer[] queueSlots;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        queueSlots = new Customer[maxQueueSize];
    }

    public int FindFirstAvailableSlot()
    {
        for (int i = 0; i < queueSlots.Length; i++)
        {
            if (queueSlots[i] == null)
                return i;
        }
        return -1; // 滿了
    }

    public bool AssignCustomerToSlot(Customer customer, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= queueSlots.Length || queueSlots[slotIndex] != null)
            return false;

        queueSlots[slotIndex] = customer;
        UpdateQueuePositions();
        return true;
    }

    public void LeaveQueue(Customer customer)
    {
        for (int i = 0; i < queueSlots.Length; i++)
        {
            if (queueSlots[i] == customer)
            {
                queueSlots[i] = null;
                ShiftQueueForward();
                UpdateQueuePositions();
                break;
            }
        }
    }

    private void ShiftQueueForward()
    {
        for (int i = 0; i < queueSlots.Length - 1; i++)
        {
            if (queueSlots[i] == null)
            {
                queueSlots[i] = queueSlots[i + 1];
                queueSlots[i + 1] = null;
            }
        }
    }

    private void UpdateQueuePositions()
    {
        for (int i = 0; i < queueSlots.Length; i++)
        {
            if (queueSlots[i] != null)
            {
                GetQueuePositionAndDirection(i, out Vector3 pos, out Vector3 faceDir);
                queueSlots[i].SetQueuePosition(pos, faceDir);
            }
        }
    }

    private void GetQueuePositionAndDirection(int index, out Vector3 position, out Vector3 faceDirection)
    {
        float distance = index * queueSpacing;

        for (int i = 0; i < queuePathPoints.Count - 1; i++)
        {
            Vector3 start = queuePathPoints[i].position;
            Vector3 end = queuePathPoints[i + 1].position;
            float segmentLength = Vector3.Distance(start, end);

            if (distance <= segmentLength)
            {
                Vector3 dir = (start - end).normalized;
                position = start + (end - start).normalized * distance;
                faceDirection = dir;
                return;
            }

            distance -= segmentLength;
        }

        position = queuePathPoints[queuePathPoints.Count - 1].position;
        faceDirection = (queuePathPoints[queuePathPoints.Count - 2].position - queuePathPoints[queuePathPoints.Count - 1].position).normalized;
    }

    // ✅ 加入的擴充功能：取得目前的排隊列表
    public List<Customer> GetCurrentQueue()
    {
        List<Customer> currentQueue = new List<Customer>();
        foreach (Customer c in queueSlots)
        {
            if (c != null)
                currentQueue.Add(c);
        }
        return currentQueue;
    }

    // ✅ 加入的擴充功能：判斷某個顧客是否在隊列中
    public bool IsInQueue(Customer customer)
    {
        foreach (Customer c in queueSlots)
        {
            if (c == customer)
                return true;
        }
        return false;
    }
}