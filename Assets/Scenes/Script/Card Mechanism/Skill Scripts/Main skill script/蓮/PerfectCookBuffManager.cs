using UnityEngine;

public class PerfectCookBuffManager : MonoBehaviour
{
    public static PerfectCookBuffManager Instance { get; private set; }

    private bool perfectBuffActive = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ActivatePerfectBuff()
    {
        perfectBuffActive = true;
        Debug.Log("✨ PerfectCookBuff 啟動，下次成功料理將自動為 Perfect！");
    }

    public void ConsumeBuff()
    {
        if (perfectBuffActive)
        {
            perfectBuffActive = false;
            Debug.Log("🌀 PerfectCookBuff 已消失。");
        }
    }

    public bool IsBuffActive()
    {
        return perfectBuffActive;
    }
}
