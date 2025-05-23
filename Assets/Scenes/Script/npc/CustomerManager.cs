using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance;

    private List<Customer> customers = new List<Customer>();
    public int CurrentCustomerCount => customers.Count;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Register(Customer c)
    {
        if (!customers.Contains(c)) customers.Add(c);
    }

    public void Unregister(Customer c)
    {
        customers.Remove(c);
    }

    public void NotifyCustomersToLeave()
    {

        foreach (var customer in customers.ToArray())
        {
            if (CustomerQueueManager.Instance != null && CustomerQueueManager.Instance.IsInQueue(customer))
            {
//                delay += interval;
            }
            else
            {
                customer.LeaveAndDespawn();
            }
        }
    }
}