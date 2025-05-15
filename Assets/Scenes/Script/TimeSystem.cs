using UnityEngine;
using UnityEngine.UI;

public class TimeSystem : MonoBehaviour
{
    public Image ringImage;
    public RectTransform pointer;
    public float cooldownDuration = 60f;

    private float timer = 0f;
    private bool running = false;
    public ModeToggleManager modeToggleManager;

    private void Update()
    {
        if (!running) return;

        timer -= Time.deltaTime;

        float t = Mathf.Clamp01(timer / cooldownDuration);

        if (ringImage != null)
            ringImage.fillAmount = t;

        if (pointer != null)
            pointer.localRotation = Quaternion.Euler(0, 0, -360 * (1 - t)); // 指針逆時針旋轉

        if (timer <= 0f)
        {
            running = false;
            modeToggleManager?.StartClosingProcessFromTimeSystem();
        }
    }

    public void StartCooldown()
    {
        timer = cooldownDuration;
        running = true;

        if (ringImage != null)
            ringImage.fillAmount = 1f;

        if (pointer != null)
            pointer.localRotation = Quaternion.Euler(0, 0, 0);
    }

    public void StopCooldown()
    {
        running = false;
    }
}
