using UnityEngine;
using UnityEngine.Rendering;

public class LightDirectionSetter : MonoBehaviour
{
    public Light mainLight;  // �O������Դ

    void Update()
    {
        if (mainLight != null)
        {
            Vector3 lightDir = -mainLight.transform.forward;
            Shader.SetGlobalVector("_MainLightDir", new Vector4(lightDir.x, lightDir.y, lightDir.z, 0));

            // �_�� `color` ����
            Shader.SetGlobalColor("_MainLightColor", mainLight.gameObject.GetComponent<Light>().color);
        }
    }
}