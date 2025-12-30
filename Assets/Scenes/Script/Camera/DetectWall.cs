using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class DetectWall : MonoBehaviour
{
    [Header("設定")]
    public Transform player;            // 玩家
    public LayerMask wallLayer;         // 遮擋物圖層
    [Range(0f, 1f)]
    public float transparentAlpha = 0.3f;  // 透明度

    // 追蹤已透明化物件與原材質
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private List<Renderer> transparentRenderers = new List<Renderer>();

    void Update()
    {
        Vector3 dir = player.position - transform.position;
        float distance = dir.magnitude;

        // 射線檢測遮擋
        RaycastHit[] hits = Physics.RaycastAll(transform.position, dir.normalized, distance, wallLayer);
        bool playerVisible = hits.Length == 0;

        // 若玩家不可見，將遮擋物設為透明
        if (!playerVisible)
        {
            foreach (RaycastHit hit in hits)
            {
                Renderer rend = hit.collider.GetComponent<Renderer>();
                if (rend != null && !transparentRenderers.Contains(rend))
                {
                    SetTransparent(rend);
                    transparentRenderers.Add(rend);
                }
            }
        }

        // 若玩家可見，恢復所有透明物件
        if (playerVisible)
        {
            foreach (Renderer rend in transparentRenderers)
            {
                RestoreOriginalMaterial(rend);
            }
            transparentRenderers.Clear();
        }
    }

    void SetTransparent(Renderer rend)
    {
        // 保存原材質
        if (!originalMaterials.ContainsKey(rend))
        {
            Material[] matsCopy = new Material[rend.materials.Length];
            for (int i = 0; i < rend.materials.Length; i++)
            {
                matsCopy[i] = new Material(rend.materials[i]);
            }
            originalMaterials[rend] = matsCopy;
        }

        // 修改材質為透明
        Material[] mats = new Material[rend.materials.Length];
        for (int i = 0; i < rend.materials.Length; i++)
        {
            mats[i] = new Material(originalMaterials[rend][i]); // 複製原材質
            SetURPTransparent(mats[i]);
        }
        rend.materials = mats;
    }

    void RestoreOriginalMaterial(Renderer rend)
    {
        if (originalMaterials.ContainsKey(rend))
        {
            rend.materials = originalMaterials[rend];
            originalMaterials.Remove(rend);
        }
    }

    void SetURPTransparent(Material mat)
    {
        mat.SetFloat("_Surface", 1f); // Transparent
        mat.SetFloat("_Blend", 0f);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = (int)RenderQueue.Transparent;

        Color c = mat.color;
        c.a = transparentAlpha;
        mat.color = c;
    }
}
