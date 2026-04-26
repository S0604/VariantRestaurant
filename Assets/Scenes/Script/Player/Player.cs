using System;
using UnityEngine;

[Serializable]
public class PlayerPositionSaveData
{
    public float posX;
    public float posY;
    public float posZ;
}

public class Player : MonoBehaviour, ISaveable
{
    [Header("Save")]
    [SerializeField] private string uniqueID = "Player";

    public bool canMove = true;
    private Animator animator;
    private Vector3 lastDirection;
    private float moveSpeed = 8f;
    public bool isCooking = false;

    [Header("對話鎖定")]
    public bool isLocked = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isLocked) return;

        canMove = !isCooking;
        if (!canMove) return;

        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(inputX, 0, inputY);

        if (direction.magnitude > 0)
        {
            if (animator != null)
                animator.SetBool("Ismoving", true);

            lastDirection = direction.normalized;
            MovePlayer(direction);
        }
        else
        {
            if (animator != null)
                animator.SetBool("Ismoving", false);
        }

        if (animator != null)
        {
            animator.SetFloat("InputX", lastDirection.x);
            animator.SetFloat("InputY", lastDirection.z);
        }
    }

    private void MovePlayer(Vector3 direction)
    {
        transform.position += direction.normalized * moveSpeed * Time.deltaTime;
    }

    public string GetUniqueID()
    {
        return uniqueID;
    }

    public string CaptureAsJson()
    {
        PlayerPositionSaveData data = new PlayerPositionSaveData
        {
            posX = transform.position.x,
            posY = transform.position.y,
            posZ = transform.position.z
        };

        return JsonUtility.ToJson(data);
    }

    public void RestoreFromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return;

        PlayerPositionSaveData data = JsonUtility.FromJson<PlayerPositionSaveData>(json);
        if (data == null)
            return;

        transform.position = new Vector3(data.posX, data.posY, data.posZ);
        Debug.Log("[Save] Player 位置已還原");
    }
}