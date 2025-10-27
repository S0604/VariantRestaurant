using UnityEngine;

public class SkillCoroutineRunner : MonoBehaviour
{
    private static SkillCoroutineRunner instance;
    public static SkillCoroutineRunner Instance
    {
        get
        {
            if (instance == null)
            {
                var obj = new GameObject("SkillCoroutineRunner");
                instance = obj.AddComponent<SkillCoroutineRunner>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        if (transform.parent != null)
        {
            transform.SetParent(null); // 🔧 確保是 root 物件
        }

        DontDestroyOnLoad(gameObject);
    }
}