using UnityEngine;
using UnityEngine.Rendering;

public class LightDirectionSetter : MonoBehaviour
{
    public Light mainLight;  // 設定主光源

    void Update()
    {
        if (mainLight != null)
        {
            Vector3 lightDir = -mainLight.transform.forward;
            Shader.SetGlobalVector("_MainLightDir", new Vector4(lightDir.x, lightDir.y, lightDir.z, 0));

            // 確保 `color` 存在
            Shader.SetGlobalColor("_MainLightColor", mainLight.gameObject.GetComponent<Light>().color);
        }
    }
}