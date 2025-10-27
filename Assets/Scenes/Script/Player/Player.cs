using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool canMove = true;
    private Animator animator;
    private Vector3 lastDirection;
    private float moveSpeed = 8f;
    public bool isCooking = false;
    public bool isLocked = false; // 外部（對話系統）用來鎖住玩家控制
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isLocked || isCooking)
            return; // 🔒 被鎖住或正在烹飪就不能動

        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(inputX, 0, inputY);

        if (direction.magnitude > 0)
        {
            animator.SetBool("Ismoving", true);
            lastDirection = direction.normalized;
            MovePlayer(direction);
        }
        else
        {
            animator.SetBool("Ismoving", false);
        }

        animator.SetFloat("InputX", lastDirection.x);
        animator.SetFloat("InputY", lastDirection.z);
    }
    private void MovePlayer(Vector3 direction)
    {
        transform.position += direction.normalized * moveSpeed * Time.deltaTime;
    }


}