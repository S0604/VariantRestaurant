using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

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

    private void Start()
    {
        customerOrder = GetComponent<CustomerOrder>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (CustomerQueueManager.Instance != null)
            CustomerQueueManager.Instance.JoinQueue(this);

        // ✅ 通知 ModeToggleManager 有新顧客
        if (ModeToggleManager.Instance != null)
            ModeToggleManager.Instance.RegisterCustomer(this);
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
                ModeToggleManager.Instance?.UnregisterCustomer(this);
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

        StartCoroutine(WaitUntilOutOfViewOrReachedSpawnPointAndDestroy());
    }

    private IEnumerator WaitUntilOutOfViewOrReachedSpawnPointAndDestroy()
    {
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend == null)
        {
            ModeToggleManager.Instance?.UnregisterCustomer(this);
            Destroy(gameObject);
            yield break;
        }

        while (true)
        {
            if (agent != null && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !agent.hasPath)
            {
                ModeToggleManager.Instance?.UnregisterCustomer(this);
                Destroy(gameObject);
                yield break;
            }

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(targetCamera);
            Bounds bounds = rend.bounds;

            if (!GeometryUtility.TestPlanesAABB(planes, bounds))
            {
                ModeToggleManager.Instance?.UnregisterCustomer(this);
                Destroy(gameObject);
                yield break;
            }

            yield return null;
        }
    }
}
