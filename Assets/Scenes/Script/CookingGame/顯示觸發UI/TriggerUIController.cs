using UnityEngine;

public class TriggerUIController : MonoBehaviour
{
    [Header("要顯示的UI物件")]
    public GameObject targetUI;

    private void Start()
    {
        if (targetUI != null)
            targetUI.SetActive(false); // 一開始先關閉
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (targetUI != null)
                targetUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (targetUI != null)
                targetUI.SetActive(false);
        }
    }
}
