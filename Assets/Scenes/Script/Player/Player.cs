using UnityEngine;

public class Player : MonoBehaviour
{
    private Animator animator;
    private Vector3 lastDirection; // 用于记录最后的移动方向
    private float moveSpeed = 4f;  // 移动速度

    void Start()
    {
        // 获取 Animator 组件
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 获取输入方向
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        // 构造方向向量
        Vector3 direction = new Vector3(inputX, 0, inputY);

        // 判断是否有输入方向
        if (direction.magnitude > 0)
        {
            // 标记为移动状态
            animator.SetBool("Ismoving", true);

            // 更新最后方向，用于动画参数
            lastDirection = direction.normalized;

            // 移动角色
            MovePlayer(direction);
        }
        else
        {
            // 停止移动动画
            animator.SetBool("Ismoving", false);
        }

        // 更新动画方向参数
        animator.SetFloat("InputX", lastDirection.x);
        animator.SetFloat("InputY", lastDirection.z);
    }

    private void MovePlayer(Vector3 direction)
    {
        // 根据方向移动角色，速度由 moveSpeed 控制
        transform.position += direction.normalized * moveSpeed * Time.deltaTime;
    }
}
