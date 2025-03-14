using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
            // 让 UI 始终朝向摄像机
            transform.forward = mainCamera.transform.forward;
        }
    }
}
