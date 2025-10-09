using UnityEngine;

/// <summary>
/// 將指定 Camera 的 RenderTexture 擷取為 Sprite（含透明背景）
/// （保留給需要「立即」擷取的場合；一般使用 CaptureRigController.CaptureEndOfFrame）
/// </summary>
public static class CaptureUtility
{
    public static Sprite CaptureToSprite(Camera targetCamera, RenderTexture rt, float pixelsPerUnit = 100f)
    {
        if (targetCamera == null || rt == null)
        {
            Debug.LogError("[CaptureUtility] targetCamera 或 RenderTexture 為空。");
            return null;
        }

        var prevActive = RenderTexture.active;
        var prevTarget = targetCamera.targetTexture;

        try
        {
            targetCamera.targetTexture = rt;
            targetCamera.Render();

            RenderTexture.active = rt;

            // 非 linear 貼圖（sRGB），避免色偏
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply(false, false);

            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                 new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }
        finally
        {
            targetCamera.targetTexture = prevTarget;
            RenderTexture.active = prevActive;
        }
    }

    public static void SetLayerRecursively(Transform root, int layer)
    {
        if (root == null) return;
        root.gameObject.layer = layer;
        for (int i = 0; i < root.childCount; i++)
            SetLayerRecursively(root.GetChild(i), layer);
    }
}
