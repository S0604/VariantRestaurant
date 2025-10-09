using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BurgerCaptureAdapter : MonoBehaviour
{
    [System.Serializable]
    public struct LayerData
    {
        public Sprite sprite;
        public int index; // 自下而上
    }

    [Header("References")]
    public CaptureRigController captureRigPrefab;

    [Header("Layout")]
    public float stackItemSpacing = 40f;
    public bool setNativeSize = true;
    public Vector2 uniformSize = new Vector2(256, 64);
    public Vector2 itemPivot = new Vector2(0.5f, 0.0f);

    [Header("Lifecycle")]
    public bool keepRigInstance = true;
    public bool debugLog = false;
    public bool debugAddWhiteBox = false; // 測試用

    CaptureRigController _rig;

    public IEnumerator CaptureSprite(List<LayerData> builtLayers, Action<Sprite> onCaptured)
    {
        if (builtLayers == null || builtLayers.Count == 0)
        {
            Debug.LogWarning("[BurgerCaptureAdapter] builtLayers is empty.");
            onCaptured?.Invoke(null); yield break;
        }

        if (_rig == null)
        {
            if (!captureRigPrefab)
            {
                Debug.LogError("[BurgerCaptureAdapter] captureRigPrefab missing.");
                onCaptured?.Invoke(null); yield break;
            }
            _rig = Instantiate(captureRigPrefab);
            if (keepRigInstance) { DontDestroyOnLoad(_rig.gameObject); _rig.gameObject.name = "[CaptureRig_Instance]"; }
        }

        _rig.ClearStackRoot();

        if (debugAddWhiteBox)
        {
            var go = new GameObject("DEBUG_WHITE_BOX", typeof(RawImage));
            go.transform.SetParent(_rig.stackRoot, false);
            var raw = go.GetComponent<RawImage>();
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            var cols = new Color32[64 * 64];
            for (int i = 0; i < cols.Length; i++) cols[i] = new Color32(255, 0, 255, 255);
            tex.SetPixels32(cols); tex.Apply(false, false);
            raw.texture = tex;
            var rt = raw.rectTransform; rt.pivot = itemPivot; rt.sizeDelta = new Vector2(200, 80); rt.anchoredPosition = new Vector2(0, 0);
        }

        // 重建層（自下而上）
        for (int i = 0; i < builtLayers.Count; i++)
        {
            var ld = builtLayers[i];
            if (!ld.sprite) continue;

            var go = new GameObject($"Layer_{ld.index}", typeof(Image));
            go.transform.SetParent(_rig.stackRoot, false);
            var img = go.GetComponent<Image>();
            img.sprite = ld.sprite;
            img.preserveAspect = true;

            var rt = img.rectTransform;
            rt.pivot = itemPivot;
            if (setNativeSize) img.SetNativeSize();
            else rt.sizeDelta = uniformSize;
            rt.anchoredPosition = new Vector2(0f, ld.index * stackItemSpacing);
        }

        _rig.ApplyCaptureLayer(); // 確保層級正確
        if (debugLog) Debug.Log($"[Adapter] builtLayers={builtLayers.Count}, stackChildren={_rig.stackRoot.childCount}");

        // EndOfFrame 擷取（最穩）
        Sprite sprite = null;
        yield return _rig.CaptureEndOfFrame(s => sprite = s);

        if (debugLog) Debug.Log($"[Adapter] captured={(sprite != null)} size={(sprite ? sprite.rect.size.ToString() : "null")}");

        if (!keepRigInstance) { Destroy(_rig.gameObject); _rig = null; }

        onCaptured?.Invoke(sprite);
    }

    public IEnumerator CaptureAndAddToInventory(List<LayerData> builtLayers, BaseMinigame.DishGrade grade, string itemName, string itemTag = "Burger")
    {
        Sprite result = null;
        yield return CaptureSprite(builtLayers, s => result = s);
        if (result != null && InventoryManager.Instance != null)
            InventoryManager.Instance.AddItemFromSprite(result, itemName, itemTag, grade);
    }
}
