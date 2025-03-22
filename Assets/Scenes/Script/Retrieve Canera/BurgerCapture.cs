using UnityEngine;
using System.Collections;
using System.IO;

public class BurgerCapture : MonoBehaviour
{

    public Camera captureCamera;
    private int screenshotIndex;

    private void Start()
    {
        screenshotIndex = PlayerPrefs.GetInt("ScreenshotIndex", 0);
    }

    public void CaptureStackPanel(string recipeKey)
    {
        StartCoroutine(CaptureCoroutine());
    }

    private IEnumerator CaptureCoroutine()
    {
        yield return new WaitForEndOfFrame();

        int w = 512, h = 512;
        RenderTexture rt = new RenderTexture(w, h, 24);
        captureCamera.targetTexture = rt;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);

        captureCamera.Render();
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();

        string dir = Application.persistentDataPath + "/Screenshots/";
        Directory.CreateDirectory(dir);

        string filename = "BurgerScreenshot" + screenshotIndex + ".png";
        string path = Path.Combine(dir, filename);
        File.WriteAllBytes(path, tex.EncodeToPNG());

        RenderTexture.active = null;
        captureCamera.targetTexture = null;
        Destroy(rt);
        Destroy(tex);

        PlayerPrefs.SetInt("ScreenshotIndex", ++screenshotIndex);

        var newUI = FindObjectOfType<NewRecipeUI>();
        if (newUI != null)
            newUI.SetRecipeImage(path);
    }
}
