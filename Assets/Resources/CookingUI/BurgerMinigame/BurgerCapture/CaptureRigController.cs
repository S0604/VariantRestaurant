using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.Experimental.Rendering; // for GraphicsFormat

public class CaptureRigController : MonoBehaviour
{
    [Header("References")]
    public Camera captureCamera;
    public Canvas captureCanvas;
    public RectTransform stackRoot;

    [Header("RenderTexture Output")]
    public int outputWidth = 512;
    public int outputHeight = 512;
    public float pixelsPerUnit = 100f;
    public RenderTexture renderTexture; // 可留空，會在 Awake 時自動建立

    [Header("Layer")]
    [Tooltip("要拍攝的專用圖層名稱，例如：BurgerCapture")]
    public string captureLayerName = "BurgerCapture";

    [Header("Post Process")]
    [Tooltip("裁掉四周透明邊，以移除多餘空白（建議開）")]
    public bool trimTransparentBorder = true;
    [Tooltip("是否將結果重採樣為固定像素大小（不再被 UI 撐滿）")]
    public bool resampleToFixedSize = true;
    [Tooltip("重採樣後的最終像素尺寸（Inspector 可調）")]
    public Vector2Int finalOutputSize = new Vector2Int(128, 128);
    public enum ScaleMode { Fit, Fill, Stretch }
    [Tooltip("Fit：含空白信封置中；Fill：滿版可能裁切；Stretch：不保比例")]
    public ScaleMode finalScaleMode = ScaleMode.Fit;

    int _captureLayer = -1;

    void Awake()
    {
        if (!captureCamera || !captureCanvas || !stackRoot)
        {
            Debug.LogError("[CaptureRigController] references missing.");
            enabled = false; return;
        }

        // === 建立 sRGB 的 RenderTexture（Linear 專案避免變暗） ===
        var desc = new RenderTextureDescriptor(outputWidth, outputHeight)
        {
            depthBufferBits = 24,
            msaaSamples = 1,
            graphicsFormat = (QualitySettings.activeColorSpace == ColorSpace.Linear)
                ? GraphicsFormat.R8G8B8A8_SRGB
                : GraphicsFormat.R8G8B8A8_UNorm,
            sRGB = (QualitySettings.activeColorSpace == ColorSpace.Linear)
        };

        if (renderTexture != null) { try { renderTexture.Release(); } catch { } }
        renderTexture = new RenderTexture(desc)
        {
            name = "[CaptureRig] RT_sRGB",
            useMipMap = false,
            autoGenerateMips = false
        };
        renderTexture.Create();

        captureCamera.targetTexture = renderTexture;

        // === 安全設定：Screen Space - Camera、透明背景 ===
        captureCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        captureCanvas.worldCamera = captureCamera;
        captureCanvas.planeDistance = 1f;

        captureCamera.enabled = true;
        captureCamera.clearFlags = CameraClearFlags.SolidColor;
        captureCamera.backgroundColor = new Color(0, 0, 0, 0); // 透明
        captureCamera.nearClipPlane = 0.1f;
        captureCamera.farClipPlane = 1000f;
        captureCamera.transform.position = new Vector3(0, 0, -10);
        captureCamera.transform.rotation = Quaternion.identity;

        // === 層級設定：相機只拍 BurgerCapture，Canvas/StackRoot 也切到同層 ===
        _captureLayer = LayerMask.NameToLayer(captureLayerName);
        if (_captureLayer >= 0)
        {
            captureCamera.cullingMask = (1 << _captureLayer);
            captureCanvas.gameObject.layer = _captureLayer;
            stackRoot.gameObject.layer = _captureLayer;
            SetLayerRecursively(stackRoot, _captureLayer);
        }
        else
        {
            Debug.LogWarning($"[CaptureRigController] Layer '{captureLayerName}' not found. Using current cullingMask.");
        }
    }

    public void ClearStackRoot()
    {
        for (int i = stackRoot.childCount - 1; i >= 0; i--)
        {
            var c = stackRoot.GetChild(i);
            if (Application.isPlaying) Destroy(c.gameObject);
            else DestroyImmediate(c.gameObject);
        }
    }

    public void ApplyCaptureLayer()
    {
        if (_captureLayer >= 0)
        {
            captureCanvas.gameObject.layer = _captureLayer;
            stackRoot.gameObject.layer = _captureLayer;
            SetLayerRecursively(stackRoot, _captureLayer);
        }
    }

    // 建議使用：等待該幀完整渲染後擷取，並處理裁切/固定尺寸
    public IEnumerator CaptureEndOfFrame(Action<Sprite> onDone)
    {
        // 讓 UI 幾何穩定
        yield return null; Canvas.ForceUpdateCanvases();
        yield return null; Canvas.ForceUpdateCanvases();

        // 等待該幀結束；Linear 專案確保往 sRGB RT 正確寫入
        yield return new WaitForEndOfFrame();

        bool prevSrgb = GL.sRGBWrite;
        GL.sRGBWrite = (QualitySettings.activeColorSpace == ColorSpace.Linear);

        captureCamera.Render();

        GL.sRGBWrite = prevSrgb;

        var rt = captureCamera.targetTexture;
        var prev = RenderTexture.active;
        try
        {
            RenderTexture.active = rt;

            // 非 linear 的 Texture2D（sRGB），避免顏色再次被轉換
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply(false, false);

            if (trimTransparentBorder)
                tex = CropAlpha(tex, 4);

            if (resampleToFixedSize)
                tex = ResampleToBox(tex, Mathf.Max(1, finalOutputSize.x), Mathf.Max(1, finalOutputSize.y), finalScaleMode);

            var sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                   new Vector2(0.5f, 0.5f), pixelsPerUnit);
            onDone?.Invoke(sp);
        }
        finally
        {
            RenderTexture.active = prev;
        }
    }

    // ===== 工具：裁透明邊 =====
    static Texture2D CropAlpha(Texture2D src, byte alphaThreshold = 4)
    {
        int w = src.width, h = src.height;
        var px32 = src.GetPixels32();   // 用來找邊界

        int minX = w, minY = h, maxX = -1, maxY = -1;

        for (int y = 0; y < h; y++)
        {
            int row = y * w;
            for (int x = 0; x < w; x++)
            {
                if (px32[row + x].a > alphaThreshold)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        // 全透明：直接回原圖
        if (maxX < minX || maxY < minY) return src;

        int nw = maxX - minX + 1;
        int nh = maxY - minY + 1;

        var outTex = new Texture2D(nw, nh, TextureFormat.RGBA32, false, false);
        var sub = src.GetPixels(minX, minY, nw, nh); // Color[]
        outTex.SetPixels(sub);
        outTex.Apply(false, false);

        UnityEngine.Object.Destroy(src);
        return outTex;
    }

    // ===== 工具：重採樣到固定尺寸（Fit / Fill / Stretch）=====
    static Texture2D ResampleToBox(Texture2D src, int outW, int outH, ScaleMode mode)
    {
        // 背景透明
        var outTex = new Texture2D(outW, outH, TextureFormat.RGBA32, false, false);
        var clear = new Color(0, 0, 0, 0);
        var fill = outTex.GetPixels();
        for (int i = 0; i < fill.Length; i++) fill[i] = clear;
        outTex.SetPixels(fill);

        int srcW = src.width, srcH = src.height;

        int dstW, dstH;
        if (mode == ScaleMode.Stretch)
        {
            dstW = outW; dstH = outH;
        }
        else
        {
            float sx = (float)outW / srcW;
            float sy = (float)outH / srcH;
            float s = (mode == ScaleMode.Fit) ? Mathf.Min(sx, sy) : Mathf.Max(sx, sy);
            dstW = Mathf.Max(1, Mathf.RoundToInt(srcW * s));
            dstH = Mathf.Max(1, Mathf.RoundToInt(srcH * s));
        }

        int offsetX = (outW - dstW) / 2;
        int offsetY = (outH - dstH) / 2;

        // 雙線性取樣
        for (int y = 0; y < dstH; y++)
        {
            float v = (y + 0.5f) / dstH; // 0..1
            for (int x = 0; x < dstW; x++)
            {
                float u = (x + 0.5f) / dstW; // 0..1
                Color c = src.GetPixelBilinear(u, v);
                outTex.SetPixel(offsetX + x, offsetY + y, c);
            }
        }

        outTex.Apply(false, false);

        UnityEngine.Object.Destroy(src);
        return outTex;
    }

    static void SetLayerRecursively(Transform root, int layer)
    {
        if (!root) return;
        root.gameObject.layer = layer;
        for (int i = 0; i < root.childCount; i++)
            SetLayerRecursively(root.GetChild(i), layer);
    }
}
