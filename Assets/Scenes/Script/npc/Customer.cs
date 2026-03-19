using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Customer : MonoBehaviour
{
    public Transform spawnPoint;
    public NavMeshAgent agent;
    public Animator animator;
    public MenuDatabase menuDatabase;
    public Camera targetCamera;

    private Vector3 faceDirection = Vector3.forward;
    private bool isIdle = false;
    private bool isLeaving = false;

    private CustomerOrder customerOrder;
    private bool hasGeneratedOrder = false;
    private float idleTimer = 0f;

    private bool waitingToJoinQueue = true;
    public bool IsSpecialCustomer => CompareTag("SpecialCustomer");

    private void Start()
    {
        customerOrder = GetComponent<CustomerOrder>();
        if (targetCamera == null)
            targetCamera = Camera.main;

        ModeToggleManager.Instance?.RegisterCustomer(this);
    }

    private void Update()
    {
        if (agent == null || animator == null) return;

        // 排隊階段
        if (waitingToJoinQueue)
        {
            TryJoinQueue();
            return;
        }

        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);

        // ===== 到達目的地 =====
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !agent.hasPath)
        {
            // ✅ 離開流程：到達 spawnPoint 才銷毀
            if (isLeaving)
            {
                if (Vector3.Distance(transform.position, spawnPoint.position) < 0.5f)
                {
                    ModeToggleManager.Instance?.UnregisterCustomer(this);
                    Destroy(gameObject);
                }
                return;
            }

            // ===== 一般站立 =====
            if (!isIdle)
            {
                animator.SetFloat("BlendX", faceDirection.x);
                animator.SetFloat("BlendY", faceDirection.z);
                isIdle = true;
                idleTimer = 0f;
            }
            else if (!hasGeneratedOrder)
            {
                idleTimer += Time.deltaTime;
                if (idleTimer >= 0.5f && customerOrder != null && menuDatabase != null)
                {
                    customerOrder.GenerateOrder(menuDatabase, IsSpecialCustomer);
                    hasGeneratedOrder = true;
                }
            }
        }
        else
        {
            // 移動中
            Vector3 dir = agent.velocity.normalized;
            animator.SetFloat("BlendX", dir.x);
            animator.SetFloat("BlendY", dir.z);
            isIdle = false;
            idleTimer = 0f;
        }
    }

    private void TryJoinQueue()
    {
        if (ModeToggleManager.Instance != null && ModeToggleManager.Instance.IsClosingPhase)
        {
            LeaveImmediately();
            return;
        }

        int slot = CustomerQueueManager.Instance.FindFirstAvailableSlot();
        if (slot != -1)
        {
            if (CustomerQueueManager.Instance.AssignCustomerToSlot(this, slot))
                waitingToJoinQueue = false;
        }
    }

    public void SetQueuePosition(Vector3 position, Vector3 faceDir)
    {
        if (agent != null)
            agent.SetDestination(position);

        faceDirection = faceDir;
        isIdle = false;
        idleTimer = 0f;
    }

    public void ReceiveOrder()
    {
        LeaveImmediately();
    }

    // ✅ 修改重點
    public void LeaveImmediately()
    {
        if (isLeaving) return;
        isLeaving = true;

        CustomerQueueManager.Instance?.LeaveQueue(this);

        if (agent != null && spawnPoint != null)
        {
            agent.isStopped = false;
            agent.SetDestination(spawnPoint.position);
        }
    }

    public void ForceLeaveAndDespawn()
    {
        LeaveImmediately();
    }

    public void LeaveAndDespawn()
    {
        LeaveImmediately();
    }
}