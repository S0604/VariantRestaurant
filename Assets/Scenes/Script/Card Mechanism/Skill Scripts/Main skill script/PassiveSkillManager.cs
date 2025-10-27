using UnityEngine;

public class PassiveSkillManager : MonoBehaviour
{
    public static PassiveSkillManager Instance { get; private set; }

    public float maxPatienceBonus = 0f;

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
}
