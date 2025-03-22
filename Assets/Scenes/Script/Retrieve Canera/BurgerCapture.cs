using UnityEngine;
using System.Collections;
using System.IO;

public class BurgerCapture : MonoBehaviour
{
    public Camera captureCamera; // 专门用来截图的相机
    public string screenshotBaseName = "BurgerScreenshot"; // 截图文件基础名称
    private int screenshotIndex;

    private void Start()
    {
        // 读取上次的截图索引，确保编号不会重复
        screenshotIndex = PlayerPrefs.GetInt("ScreenshotIndex", 0);
    }

    public void CaptureStackPanel(string recipeKey)
    {
        StartCoroutine(CaptureRenderTextureCoroutine());
    }

    private IEnumerator CaptureRenderTextureCoroutine()
    {
        yield return new WaitForEndOfFrame();

        int width = 512;  // 固定截图大小，避免 UI 过大
        int height = 512;

        RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.Default);
        captureCamera.targetTexture = rt;
        captureCamera.clearFlags = CameraClearFlags.SolidColor;
        captureCamera.backgroundColor = new Color(0, 0, 0, 0);
        captureCamera.Render();

        RenderTexture.active = rt;
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGBA32, false);
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply(); // 确保贴图更新

        // 确保文件夹存在
        string directoryPath = Application.persistentDataPath + "/Screenshots/";
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // 生成唯一文件名
        string screenshotName = $"{screenshotBaseName}{screenshotIndex}.png";
        string path = Path.Combine(directoryPath, screenshotName);

        // 保存 PNG
        byte[] bytes = screenshot.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Debug.Log($"📸 截图已保存: {path}");

        // 释放资源
        captureCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        Destroy(screenshot);

        // 刷新 Unity 资源数据库
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

        // 更新索引并存储
        screenshotIndex++;
        PlayerPrefs.SetInt("ScreenshotIndex", screenshotIndex);
        PlayerPrefs.Save();

        // 显示新料理 UI
        NewRecipeUI newRecipeUI = FindObjectOfType<NewRecipeUI>();
        if (newRecipeUI != null)
        {
            newRecipeUI.SetRecipeImage(path);
        }
        else
        {
            Debug.LogError("❌ NewRecipeUI 未找到，无法显示新料理图片！");
        }
    }
}
