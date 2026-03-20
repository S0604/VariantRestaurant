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

        FreeModeToggleManager.Instance?.RegisterCustomer(this);
    }

    private void Update()
    {
        if (agent == null || animator == null) return;

        if (waitingToJoinQueue)
        {
            TryJoinQueue();
            return;
        }

        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !agent.hasPath)
        {
            // ===== 離場完成 =====
            if (isLeaving)
            {
                if (Vector3.Distance(transform.position, spawnPoint.position) < 0.5f)
                {
                    Despawn();
                }
                return;
            }

            // ===== Idle =====
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
            Vector3 dir = agent.velocity.normalized;
            animator.SetFloat("BlendX", dir.x);
            animator.SetFloat("BlendY", dir.z);
            isIdle = false;
            idleTimer = 0f;
        }
    }

    private void TryJoinQueue()
    {
        if (FreeModeToggleManager.Instance != null && FreeModeToggleManager.Instance.IsClosingPhase)
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

    // ===== 離場開始 =====
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

        // 保險：5秒後強制消失
        StartCoroutine(AutoDespawnFallback(5f));
    }

    private IEnumerator AutoDespawnFallback(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (isLeaving)
        {
            Debug.LogWarning($"Fallback Despawn: {name}");
            Despawn();
        }
    }

    // ===== 統一銷毀出口 =====
    private void Despawn()
    {
        FreeModeToggleManager.Instance?.OnCustomerDespawn(this);
        Destroy(gameObject);
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