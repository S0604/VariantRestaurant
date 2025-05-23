using UnityEngine;
using UnityEngine.UI;

public class TimeSystem : MonoBehaviour
{
    public Image fillCircle;
    public Transform needle;

    public void UpdateTimeVisual(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);
        if (fillCircle != null)
            fillCircle.fillAmount = normalized;

        if (needle != null)
            needle.localEulerAngles = new Vector3(0, 0, 360f * normalized);
    }

    public void ResetTimeVisual()
    {
        UpdateTimeVisual(1f);
    }
}
