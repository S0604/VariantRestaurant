using UnityEngine.AI;
using UnityEngine;

public class Customer : MonoBehaviour
{
    public Transform spawnPoint;
    public NavMeshAgent agent;
    public Animator animator;
    public MenuDatabase menuDatabase;

    private Vector3 faceDirection = Vector3.forward;
    private bool isIdle = false;

    private CustomerOrder customerOrder;
    private bool hasGeneratedOrder = false;
    private float idleTimer = 0f;

    private void Start()
    {
        customerOrder = GetComponent<CustomerOrder>();

        if (CustomerQueueManager.Instance != null)
            CustomerQueueManager.Instance.JoinQueue(this);
    }

    private void Update()
    {
        if (agent == null || animator == null) return;

        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !agent.hasPath)
        {
            if (!isIdle)
            {
                animator.SetFloat("BlendX", faceDirection.x);
                animator.SetFloat("BlendY", faceDirection.z);
                isIdle = true;
                idleTimer = 0f;
            }
            else
            {
                idleTimer += Time.deltaTime;

                if (!hasGeneratedOrder && idleTimer >= 0.5f)
                {
                    if (customerOrder != null && menuDatabase != null)
                    {
                        customerOrder.GenerateOrder(menuDatabase);
                        hasGeneratedOrder = true;
                    }
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

    public void SetQueuePosition(Vector3 position, Vector3 faceDir)
    {
        if (agent != null)
            agent.SetDestination(position);

        faceDirection = faceDir;
        isIdle = false;
        idleTimer = 0f;
    }

    public void LeaveAndDespawn()
    {
        CustomerQueueManager.Instance?.LeaveQueue(this);

        if (agent != null && spawnPoint != null)
        {
            agent.SetDestination(spawnPoint.position);
        }

        Destroy(gameObject, 3f);
        Debug.Log($"【Customer】{name} 正在離開，準備刪除");

    }

}
