using System.Collections.Generic;
using UnityEngine;

public class CustomerQueueManager : MonoBehaviour
{
    public static CustomerQueueManager Instance;

    public List<Transform> queuePathPoints;
    public float queueSpacing = 1.5f;
    public int maxQueueSize = 5;

    private List<Customer> customersInQueue = new List<Customer>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void JoinQueue(Customer customer)
    {
        if (customersInQueue.Count >= maxQueueSize)
        {
            customer.LeaveAndDespawn();
            return;
        }

        customersInQueue.Add(customer);
        UpdateQueuePositions();
    }

    public void LeaveQueue(Customer customer)
    {
        if (customersInQueue.Contains(customer))
        {
            customersInQueue.Remove(customer);
            UpdateQueuePositions();
        }
    }

    public bool IsInQueue(Customer customer)
    {
        return customersInQueue.Contains(customer);
    }

    public List<Customer> GetCurrentQueue()
    {
        return new List<Customer>(customersInQueue);
    }

    private void UpdateQueuePositions()
    {
        for (int i = 0; i < customersInQueue.Count; i++)
        {
            GetQueuePositionAndDirection(i, out Vector3 pos, out Vector3 faceDir);
            customersInQueue[i].SetQueuePosition(pos, faceDir);
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
}