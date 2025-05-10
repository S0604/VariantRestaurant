using UnityEngine;
using System.Collections.Generic;

public class ObstacleTransparency : MonoBehaviour
{
    public Transform target; // 玩家
    public LayerMask obstacleLayer;
    public float transparentAlpha = 0.3f;
    public float fadeSpeed = 5f;

    private Dictionary<Renderer, float> obstacleStates = new Dictionary<Renderer, float>();

    void Update()
    {
        HashSet<Renderer> currentObstacles = new HashSet<Renderer>();

        // 發射 Raycast 找出遮擋物
        Vector3 dir = target.position - transform.position;
        float distance = dir.magnitude;
        Ray ray = new Ray(transform.position, dir.normalized);
        RaycastHit[] hits = Physics.RaycastAll(ray, distance, obstacleLayer);

        foreach (RaycastHit hit in hits)
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend != null)
            {
                currentObstacles.Add(rend);
                if (!obstacleStates.ContainsKey(rend))
                {
                    obstacleStates[rend] = 1f; // 初始為不透明
                    SetMaterialTransparent(rend);
                }
            }
        }

        // 處理目前所有記錄過的遮擋物
        var renderers = new List<Renderer>(obstacleStates.Keys);
        foreach (Renderer rend in renderers)
        {
            float targetAlpha = currentObstacles.Contains(rend) ? transparentAlpha : 1f;
            float currentAlpha = obstacleStates[rend];
            float newAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

            SetAlpha(rend, newAlpha);
            obstacleStates[rend] = newAlpha;

            // 如果已經恢復為不透明，就移除記錄
            if (Mathf.Approximately(newAlpha, 1f) && !currentObstacles.Contains(rend))
            {
                ResetMaterialOpaque(rend);
                obstacleStates.Remove(rend);
            }
        }
    }

    void SetAlpha(Renderer rend, float alpha)
    {
        foreach (Material mat in rend.materials)
        {
            Color color = mat.color;
            color.a = alpha;
            mat.color = color;
        }
    }

    void SetMaterialTransparent(Renderer rend)
    {
        foreach (Material mat in rend.materials)
        {
            mat.shader = Shader.Find("Transparent/Diffuse");
        }
    }

    void ResetMaterialOpaque(Renderer rend)
    {
        foreach (Material mat in rend.materials)
        {
            mat.shader = Shader.Find("Standard");
            Color color = mat.color;
            color.a = 1f;
            mat.color = color;
        }
    }
}
