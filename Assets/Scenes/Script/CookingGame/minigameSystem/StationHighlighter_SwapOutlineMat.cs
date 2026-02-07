using System.Collections.Generic;
using UnityEngine;

public class StationHighlighter_SwapOutlineMat : MonoBehaviour
{
    [Header("Targets (empty = auto find children)")]
    [SerializeField] private Renderer[] targetRenderers;

    [Header("Outline Materials")]
    [Tooltip("平常的黑邊 Outline 材質（你的 Outline med）")]
    [SerializeField] private Material normalOutlineMat;

    [Tooltip("互動時的高亮 Outline 材質（建議複製一份 Outline，調亮/調粗）")]
    [SerializeField] private Material highlightOutlineMat;

    [Header("How to identify outline slot")]
    [Tooltip("若你確定 Outline 永遠在 materials 的第幾個位置（通常是 1），填這個；不確定就設 -1 讓它用材質比對尋找")]
    [SerializeField] private int outlineSlotIndex = 1;

    // 每個 Renderer 的原始材質陣列快照（含黑邊）
    private readonly Dictionary<Renderer, Material[]> _originalMats = new();
    // 每個 Renderer 的 Outline slot 索引
    private readonly Dictionary<Renderer, int> _outlineIndex = new();

    private void Awake()
    {
        if (targetRenderers == null || targetRenderers.Length == 0)
            targetRenderers = GetComponentsInChildren<Renderer>(true);

        CacheOriginalMaterials();
        SetHighlight(false);
    }

    private void CacheOriginalMaterials()
    {
        _originalMats.Clear();
        _outlineIndex.Clear();

        foreach (var r in targetRenderers)
        {
            if (!r) continue;

            var mats = r.sharedMaterials;
            if (mats == null || mats.Length == 0) continue;

            // 保存原始材質陣列（平常狀態應該已包含黑邊）
            var copy = new Material[mats.Length];
            mats.CopyTo(copy, 0);
            _originalMats[r] = copy;

            // 找 Outline slot
            int idx = -1;

            if (outlineSlotIndex >= 0 && outlineSlotIndex < mats.Length)
            {
                idx = outlineSlotIndex;
            }
            else if (normalOutlineMat != null)
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

            _outlineIndex[r] = idx; // idx=-1 代表找不到，就不處理該 Renderer
        }
    }

    public void SetHighlight(bool on)
    {
        foreach (var kv in _originalMats)
        {
            var r = kv.Key;
            if (!r) continue;

            int idx = _outlineIndex.TryGetValue(r, out var oi) ? oi : -1;
            if (idx < 0) continue; // 這個 Renderer 沒找到 Outline slot，跳過

            // 以「原始材質陣列」為基底，僅替換 Outline 那格
            var mats = (Material[])kv.Value.Clone();

            if (on)
            {
                if (highlightOutlineMat != null) mats[idx] = highlightOutlineMat;
            }
            else
            {
                if (normalOutlineMat != null) mats[idx] = normalOutlineMat;
                // 如果 normalOutlineMat 沒填，就回到原本快照（kv.Value）已是黑邊狀態
            }

            r.sharedMaterials = mats;
        }
    }
}
