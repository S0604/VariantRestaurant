using UnityEngine;

public class PassiveSkillManager : MonoBehaviour
{
    public static PassiveSkillManager Instance;

    [Header("耐心加成")]
    public float maxPatienceBonus = 0f;

    [Header("烹飪台能量加成")]
    public int maxEnergyBonus = 0;

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
