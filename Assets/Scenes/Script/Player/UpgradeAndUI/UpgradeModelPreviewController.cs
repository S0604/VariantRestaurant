using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeModelPreviewController : MonoBehaviour
{
    [Serializable]
    public class PreviewModel
    {
        [Tooltip("按鈕要切換用的代號，例如 A / B / Workbench / SupplyBox")]
        public string id = "A";

        [Tooltip("要顯示的模型 Prefab（建議是乾淨的美術 Prefab，不要帶場景邏輯）")]
        public GameObject prefab;

        [Header("Transform (local to pivot)")]
        public Vector3 localPosition = Vector3.zero;
        public Vector3 localEulerAngles = Vector3.zero;
        public float uniformScale = 1f;
    }

    [Header("UI Output")]
    [SerializeField] private RawImage previewImage;          // UI 上顯示用
    [SerializeField] private RenderTexture previewRT;        // RenderTexture 資源

    [Header("Preview Rig")]
    [SerializeField] private Camera previewCamera;           // 專用預覽相機
    [SerializeField] private Transform modelPivot;           // 模型掛載點 + 旋轉軸

    [Header("Rotation")]
    [SerializeField] private bool rotate = true;
    [SerializeField] private float rotateSpeedY = 20f;       // 每秒旋轉角度
    [SerializeField] private bool useUnscaledTime = true;    // timeScale=0 仍然旋轉

    [Header("Layer (recommended)")]
    [SerializeField] private bool usePreviewLayer = true;
    [SerializeField] private string previewLayerName = "Preview";

    [Header("Models")]
    [SerializeField] private List<PreviewModel> models = new List<PreviewModel>();

    private GameObject _currentInstance;
    private int _previewLayer = -1;

    void Awake()
    {
        // 綁定 RT 到 Camera / RawImage
        if (previewCamera && previewRT)
            previewCamera.targetTexture = previewRT;

        if (previewImage && previewRT)
            previewImage.texture = previewRT;

        if (usePreviewLayer)
        {
            _previewLayer = LayerMask.NameToLayer(previewLayerName);
            if (_previewLayer < 0)
            {
                Debug.LogWarning($"[UpgradeModelPreview] Layer '{previewLayerName}' not found. Disable usePreviewLayer or create the layer.");
                usePreviewLayer = false;
            }
            else if (previewCamera)
            {
                previewCamera.cullingMask = (1 << _previewLayer);
            }
        }

        // 預設先顯示第一個（可自行改掉）
        if (models.Count > 0)
            ShowByIndex(0);
    }

    void Update()
    {
        if (!rotate || modelPivot == null) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        modelPivot.Rotate(0f, rotateSpeedY * dt, 0f, Space.Self);
    }

    // ===== 給 UI Button 呼叫 =====

    public void ShowById(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        int idx = models.FindIndex(m => string.Equals(m.id, id, StringComparison.OrdinalIgnoreCase));
        if (idx < 0)
        {
            Debug.LogWarning($"[UpgradeModelPreview] Model id '{id}' not found.");
            return;
        }
        ShowByIndex(idx);
    }

    public void ShowByIndex(int index)
    {
        if (index < 0 || index >= models.Count) return;

        var m = models[index];
        if (!m.prefab)
        {
            Debug.LogWarning($"[UpgradeModelPreview] Model prefab missing at index {index}.");
            return;
        }

        ClearCurrent();

        _currentInstance = Instantiate(m.prefab, modelPivot);
        _currentInstance.transform.localPosition = m.localPosition;
        _currentInstance.transform.localRotation = Quaternion.Euler(m.localEulerAngles);
        _currentInstance.transform.localScale = Vector3.one * Mathf.Max(0.0001f, m.uniformScale);

        if (usePreviewLayer && _previewLayer >= 0)
            SetLayerRecursively(_currentInstance.transform, _previewLayer);
    }


    private void ClearCurrent()
    {
        if (_currentInstance)
        {
            Destroy(_currentInstance);
            _currentInstance = null;
        }
    }

    private static void SetLayerRecursively(Transform root, int layer)
    {
        if (!root) return;
        root.gameObject.layer = layer;
        for (int i = 0; i < root.childCount; i++)
            SetLayerRecursively(root.GetChild(i), layer);
    }
}
