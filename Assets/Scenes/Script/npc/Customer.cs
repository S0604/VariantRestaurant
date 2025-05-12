using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Customer : MonoBehaviour
{
    public Transform spawnPoint;
    public NavMeshAgent agent;
    public Animator animator;
    public MenuDatabase menuDatabase;

    [Tooltip("指定要檢查是否離開視野的攝影機。如果不指定則使用 Camera.main")]
    public Camera targetCamera;

    private Vector3 faceDirection = Vector3.forward;
    private bool isIdle = false;
    private bool isLeaving = false;

    private CustomerOrder customerOrder;
    private bool hasGeneratedOrder = false;
    private float idleTimer = 0f;

    private void Start()
    {
        customerOrder = GetComponent<CustomerOrder>();

        // 自動指定攝影機（若未設定）
        if (targetCamera == null)
            targetCamera = Camera.main;

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
            if (isLeaving && Vector3.Distance(transform.position, spawnPoint.position) < 0.5f)
            {
                Debug.Log($"【Customer】{name} 抵達重生點，強制銷毀");
                Destroy(gameObject);
                return;
            }

            if (!isIdle && !isLeaving)
            {
                animator.SetFloat("BlendX", faceDirection.x);
                animator.SetFloat("BlendY", faceDirection.z);
                isIdle = true;
                idleTimer = 0f;
            }
            else if (!hasGeneratedOrder && !isLeaving)
            {
                idleTimer += Time.deltaTime;

                if (idleTimer >= 0.5f && customerOrder != null && menuDatabase != null)
                {
                    customerOrder.GenerateOrder(menuDatabase);
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
        if (isLeaving) return;

        isLeaving = true;
        CustomerQueueManager.Instance?.LeaveQueue(this);

        if (agent != null && spawnPoint != null)
        {
            agent.SetDestination(spawnPoint.position);
        }

        // 開始協程監控顧客離開視野或到達生成點
        StartCoroutine(WaitUntilOutOfViewOrReachedSpawnPointAndDestroy());
        Debug.Log($"【Customer】{name} 正在離開，準備銷毀（等待離開視野或到達重生點）");
    }

    private IEnumerator WaitUntilOutOfViewOrReachedSpawnPointAndDestroy()
    {
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend == null)
        {
            Debug.LogWarning($"【Customer】{name} 沒有 Renderer，直接銷毀");
            Destroy(gameObject);
            yield break;
        }

        while (true)
        {
            // ✅ 使用 NavMeshAgent 的標準到達條件
            if (agent != null && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !agent.hasPath)
            {
                Debug.Log($"【Customer】{name} 已到達重生點（視野內），直接銷毀");
                Destroy(gameObject);
                yield break;
            }

            // ✅ 檢查是否離開視野
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(targetCamera);
            Bounds bounds = rend.bounds;

            if (!GeometryUtility.TestPlanesAABB(planes, bounds))
            {
                Debug.Log($"【Customer】{name} 離開攝影機視野，銷毀");
                Destroy(gameObject);
                yield break;
            }

            yield return null;
        }
    }
}