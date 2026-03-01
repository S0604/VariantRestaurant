using System.Collections.Generic;
using UnityEngine;

public class StationHighlighter_SwapOutlineMat : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("要被高亮的 Renderer。可指定多個；留空則自動抓取所有子物件 Renderer。")]
    [SerializeField] private Renderer[] targetRenderers;

    [Tooltip("若 targetRenderers 留空，是否自動抓子物件 Renderers")]
    [SerializeField] private bool autoFindChildrenIfEmpty = true;

    [Header("Outline Materials")]
    [Tooltip("平常的黑邊 Outline 材質（你的 Outline med）")]
    [SerializeField] private Material normalOutlineMat;

    [Tooltip("互動時的高亮 Outline 材質（建議複製 Outline，調亮/調粗）")]
    [SerializeField] private Material highlightOutlineMat;

    [Header("How to identify outline slot")]
    [Tooltip("若你確定 Outline 永遠在 materials 的第幾個位置（通常是 1），填這個；不確定就設 -1 讓它用材質比對尋找")]
    [SerializeField] private int outlineSlotIndex = 1;

    // 每個 Renderer 的原始材質陣列快照（平常狀態，應包含黑邊）
    private readonly Dictionary<Renderer, Material[]> _originalMats = new();
    // 每個 Renderer 的 Outline slot 索引
    private readonly Dictionary<Renderer, int> _outlineIndex = new();

    private void Awake()
    {
        ResolveTargets();
        CacheOriginalMaterials();
        SetHighlight(false);
    }

    private void OnValidate()
    {
        // 讓你在 Inspector 改目標後，Play 時更不容易踩到舊快照
        //（不在 Editor 模式頻繁操作，避免額外成本）
        if (!Application.isPlaying) return;

        ResolveTargets();
        CacheOriginalMaterials();
    }

    private void ResolveTargets()
    {
        if ((targetRenderers == null || targetRenderers.Length == 0) && autoFindChildrenIfEmpty)
        {
            targetRenderers = GetComponentsInChildren<Renderer>(true);
        }
    }

    private void CacheOriginalMaterials()
    {
        _originalMats.Clear();
        _outlineIndex.Clear();

        if (targetRenderers == null) return;

        foreach (var r in targetRenderers)
        {
            if (!r) continue;

            var mats = r.sharedMaterials;
            if (mats == null || mats.Length == 0) continue;

            // 存原始材質陣列（平常狀態）
            var copy = new Material[mats.Length];
            mats.CopyTo(copy, 0);
            _originalMats[r] = copy;

            // 找 Outline slot
            int idx = -1;

            // 1) 若指定固定 slot
            if (outlineSlotIndex >= 0 && outlineSlotIndex < mats.Length)
            {
                idx = outlineSlotIndex;
            }
            else
            {
                // 2) 若 slot 不確定，用 normalOutlineMat 比對找
                if (normalOutlineMat != null)
                {
                    for (int i = 0; i < mats.Length; i++)
                    {
                        if (mats[i] == normalOutlineMat)
                        {
                            idx = i;
                            break;
                        }
                    }
                }
            }

            _outlineIndex[r] = idx; // -1 代表找不到，就不處理該 Renderer
        }
    }

    public void SetHighlight(bool on)
    {
        if (_originalMats.Count == 0) return;

        foreach (var kv in _originalMats)
        {
            var r = kv.Key;
            if (!r) continue;

            int idx = _outlineIndex.TryGetValue(r, out var oi) ? oi : -1;
            if (idx < 0) continue;

            // 以原始材質為基底，只替換 outline 那格
            var mats = (Material[])kv.Value.Clone();

            if (on)
            {
                if (highlightOutlineMat != null)
                    mats[idx] = highlightOutlineMat;
            }
            else
            {
                // 若你有指定 normalOutlineMat，就確保回到它；否則回到原始快照
                if (normalOutlineMat != null)
                    mats[idx] = normalOutlineMat;
            }

            r.sharedMaterials = mats;
        }
    }
}