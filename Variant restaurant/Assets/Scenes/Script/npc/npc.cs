using UnityEngine;
using UnityEngine.AI;

public class npc : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;
    private Transform cameraTransform; // 摄像机的位置
    private Transform targetChair; // 目标椅子的位置
    private bool hasReachedChair = false; // 标记是否已经到达椅子

    public GameObject sittingNpcPrefab; // 坐下的NPC预设

    // 新增参数，用来控制动画方向
    private float blendX;
    private float blendY;

    // 椅子角度的调整范围
    private const float angle045 = -45f;
    private const float angle45 = 45f;          
    private const float angle135 = 135f; 
    private const float angle225 = 225f;
    private const float angle315 = 315f; 

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        cameraTransform = Camera.main.transform;  // 获取主摄像机
        targetChair = GameObject.FindWithTag("Chair").transform; // 通过Tag获取目标椅子的位置
        animator = GetComponent<Animator>();

        // 设置导航目标
        agent.SetDestination(targetChair.position);
    }

    void Update()
    {
        // 让 NPC 始终面向摄像机
        Vector3 lookDirection = new Vector3(cameraTransform.position.x, transform.position.y, cameraTransform.position.z);
        transform.LookAt(lookDirection);

        // 如果已经接近椅子，停止移动
        if (!hasReachedChair && agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.isStopped = true;
            hasReachedChair = true;
        }

        // 获取NavMesh Agent的速度并设置到Animator
        Vector3 velocity = agent.velocity;

        // 设置 Speed，控制行走和待机动画的过渡
        animator.SetFloat("Speed", velocity.magnitude);

        // 计算移动方向，传递给Blend Tree
        if (velocity.magnitude > 0.1f)
        {
            // 使用 velocity 的 x 和 z 来确定方向
            blendX = Mathf.Sign(velocity.x); // 根据速度的x分量判断方向
            blendY = Mathf.Sign(velocity.z); // 根据速度的z分量判断方向
        }
        else
        {
            blendX = 0;
            blendY = 0;
        }

        // 更新动画参数
        animator.SetFloat("BlendX", blendX);
        animator.SetFloat("BlendY", blendY);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Chair"))
        {
            Debug.Log("NPC 已接近椅子");

            // 计算椅子的方向向量
            Vector3 chairForward = other.transform.forward;  // 椅子的前方方向

            // 计算 NPC 相对椅子的方向，获取 BlendX 和 BlendY
            float dotForward = Vector3.Dot(chairForward, transform.forward);  // 水平面上的前向对比
            float dotRight = Vector3.Dot(chairForward, transform.right); // 右向对比

            // 根据 dot 值确定 X 和 Y 的方向
            blendX = dotRight > 0 ? 1 : -1;  // 根据右向确定 BlendX
            blendY = dotForward > 0 ? 1 : -1;  // 根据前向确定 BlendY

            // 销毁当前 NPC
            Destroy(this.gameObject);

            // 在椅子的位置生成坐下的NPC
            GameObject sittingNPC = Instantiate(sittingNpcPrefab, other.transform.position, Quaternion.identity);

            // 将新的 NPC 设置为椅子的子物体
            sittingNPC.transform.SetParent(other.transform);

            // 检查椅子的角度
            float chairAngle = other.transform.rotation.eulerAngles.y;

            // 根据椅子角度调整坐下 NPC 的 X 轴
            if (chairAngle >= angle045 && chairAngle < angle45)
            {
                sittingNPC.transform.localPosition += new Vector3(0.8f, 2f, 0.8f);
            }
            else if (chairAngle >= angle45 && chairAngle <= angle135)
            {
                sittingNPC.transform.localPosition += new Vector3(0.5f, 1.3f, 0.3f);
            }
            else if (chairAngle >= angle135 && chairAngle < angle225)
            {
                sittingNPC.transform.localPosition += new Vector3(-0.1f, 1.5f, 0);
            }
            else if (chairAngle >= angle225 && chairAngle < angle315 )
            {
                sittingNPC.transform.localPosition += new Vector3(-0.2f, 1.3f, 0.4f);
            }

            // 获取新NPC的 Animator，并设置参数
            Animator sittingAnimator = sittingNPC.GetComponent<Animator>();
            if (sittingAnimator != null)
            {
                sittingAnimator.SetFloat("BlendX", blendX);
                sittingAnimator.SetFloat("BlendY", blendY);
            }
        }
    }
}
