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

    // shader property id（避免字串開銷）
    private static readonly int FadeID = Shader.PropertyToID("_Fade");

    private Dictionary<Renderer, float> fadeDict = new Dictionary<Renderer, float>();
    private HashSet<Renderer> hitThisFrame = new HashSet<Renderer>();

    // 共用 MPB（避免 GC）
    private MaterialPropertyBlock mpb;

    void Awake()
    {
        mpb = new MaterialPropertyBlock();
    }

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

        RaycastHit[] hits = Physics.SphereCastAll(ray, sphereRadius, dist);

        foreach (var hit in hits)
        {
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
        List<Renderer> keys = new List<Renderer>(fadeDict.Keys);
        List<Renderer> toRemove = new List<Renderer>();

        foreach (var r in keys)
        {
            if (r == null)
            {
                toRemove.Add(r);
                continue;
            }

            float current = fadeDict[r];
            float target = hitThisFrame.Contains(r) ? targetFade : 1f;

            float newFade = Mathf.Lerp(current, target, Time.deltaTime * fadeSpeed);

            r.GetPropertyBlock(mpb);
            mpb.SetFloat(FadeID, newFade);
            r.SetPropertyBlock(mpb);

            fadeDict[r] = newFade;

            if (!hitThisFrame.Contains(r) && newFade > 0.99f)
            {
                r.GetPropertyBlock(mpb);
                mpb.SetFloat(FadeID, 1f);
                r.SetPropertyBlock(mpb);

                toRemove.Add(r);
            }
        }

        foreach (var r in toRemove)
        {
            fadeDict.Remove(r);
        }
    }
}