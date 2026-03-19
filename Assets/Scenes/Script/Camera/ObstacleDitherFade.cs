using System.Collections.Generic;
using UnityEngine;

public class ObstacleDitherFade : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public LayerMask obstacleLayer;

    [Header("Fade Settings")]
    public float targetFade = 0.3f;
    public float fadeSpeed = 10f;
    public float sphereRadius = 0.5f;

    private Dictionary<Renderer, float> fadeDict = new Dictionary<Renderer, float>();
    private HashSet<Renderer> hitThisFrame = new HashSet<Renderer>();

    void Update()
    {
        if (player == null) return;

        DetectObstacles();
        UpdateFade();
    }

    void DetectObstacles()
    {
        hitThisFrame.Clear();

        Vector3 origin = transform.position;
        Vector3 dir = player.position - origin;
        float dist = dir.magnitude;

        Ray ray = new Ray(origin, dir.normalized);

        // 🔥 改用 SphereCast（解決漏判）
        RaycastHit[] hits = Physics.SphereCastAll(ray, sphereRadius, dist);

        foreach (var hit in hits)
        {
            // 手動過濾 Layer（更穩）
            if (((1 << hit.collider.gameObject.layer) & obstacleLayer) == 0)
                continue;

            Renderer r = hit.collider.GetComponent<Renderer>();
            if (r == null) continue;

            hitThisFrame.Add(r);

            if (!fadeDict.ContainsKey(r))
                fadeDict.Add(r, 0f);
        }
    }

    void UpdateFade()
    {
        List<Renderer> toRemove = new List<Renderer>();

        foreach (var kvp in fadeDict)
        {
            Renderer r = kvp.Key;
            float current = kvp.Value;

            if (r == null)
            {
                toRemove.Add(r);
                continue;
            }

            float target = hitThisFrame.Contains(r) ? targetFade : 0f;

            float newFade = Mathf.Lerp(current, target, Time.deltaTime * fadeSpeed);

            r.material.SetFloat("_Fade", newFade);
            fadeDict[r] = newFade;

            // 🔥 保證會恢復
            if (!hitThisFrame.Contains(r) && newFade < 0.01f)
            {
                r.material.SetFloat("_Fade", 0f);
                toRemove.Add(r);
            }
        }

        foreach (var r in toRemove)
        {
            fadeDict.Remove(r);
        }
    }
}