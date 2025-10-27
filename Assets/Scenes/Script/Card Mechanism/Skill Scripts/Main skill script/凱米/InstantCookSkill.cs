using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "InstantCookSkill", menuName = "Skills/Active/InstantCook")]
public class InstantCookSkill : ActiveSkill
{
    public float duration = 10f; // 技能持續秒數

    public override void Activate(GameObject player)
    {
        Debug.Log($"🔥 使用主動技：{skillName} → {duration} 秒內烹飪台自動完成料理");

        SkillCoroutineRunner.Instance.StartCoroutine(EnableInstantCookAllStations(duration));
    }

    private IEnumerator EnableInstantCookAllStations(float duration)
    {
        var stations = Object.FindObjectsOfType<CookingStation>();
        foreach (var station in stations)
        {
            station.EnableInstantCook(duration);
        }

        Debug.Log($"✅ 所有烹飪台啟動自動完成模式（持續 {duration} 秒）");

        yield return new WaitForSeconds(duration);

        Debug.Log("⏳ 自動完成效果結束");
    }
}
