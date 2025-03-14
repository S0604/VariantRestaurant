using UnityEngine;
using System.Collections;
using System.IO;

public class BurgerCapture : MonoBehaviour
{
    public Camera captureCamera; // 专门用来截图的相机
    public string screenshotBaseName = "BurgerScreenshot"; // 截图文件基础名称
    private int screenshotIndex = 0; // 截图编号

    public void CaptureStackPanel()
    {
        StartCoroutine(CaptureRenderTextureCoroutine());
    }

    private IEnumerator CaptureRenderTextureCoroutine()
    {
        yield return new WaitForEndOfFrame();

        int width = Screen.width;
        int height = Screen.height;

        // 创建 RenderTexture
        RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        captureCamera.targetTexture = rt;
        captureCamera.clearFlags = CameraClearFlags.SolidColor;
        captureCamera.backgroundColor = new Color(0, 0, 0, 0); // 透明背景
        captureCamera.Render();

        // 读取 RenderTexture 数据
        RenderTexture.active = rt;
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGBA32, false);
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);

        // 确保文件夹存在
        string directoryPath = Application.dataPath + "/Resources/Screenshots/";
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // 生成唯一文件名
        string screenshotName = screenshotBaseName + screenshotIndex + ".png";
        string path = Path.Combine(directoryPath, screenshotName);

        // 保存 PNG
        byte[] bytes = screenshot.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Debug.Log("Screenshot saved at: " + path);

        // 释放资源
        captureCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        Destroy(screenshot);

        // 刷新 Unity 资源数据库
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

        screenshotIndex++;

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
