using UnityEngine;

public class PointerRotation : MonoBehaviour
{
    public TimeSystem timeSystem;         // 拖入你的 TimeSystem 腳本
    public float fullRotationAngle = 360f; // 指針一圈的角度（可設180f等）
    public bool clockwise = true;          // 是否順時針旋轉

    private RectTransform pointerRect;

    private void Start()
    {
        pointerRect = GetComponent<RectTransform>();
        if (timeSystem == null)
        {
            Debug.LogWarning("未設定 TimeSystem，指針無法旋轉。");
        }
    }

    private void Update()
    {
        if (timeSystem == null || timeSystem.cooldownDuration <= 0f) return;

        float ratio = Mathf.Clamp01(timeSystem.cooldownDuration / timeSystem.cooldownDuration);

        float angle = ratio * fullRotationAngle;
        if (!clockwise) angle = -angle;

        pointerRect.localEulerAngles = new Vector3(0f, 0f, angle);
    }
}
