using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Light : MonoBehaviour
{
    public Light mainLight; // 指定主光源（Directional Light）
    public Material targetMaterial; // 指定要影響的材質

    void Update()
    {
        if (mainLight != null && targetMaterial != null)
        {
            // 取得光照方向（Unity 的 Directional Light 的 forward 方向與光照方向相反）
            Vector3 lightDir = -mainLight.transform.forward;

            // 將光照方向傳遞給 Shader
            targetMaterial.SetVector("_LightDir", lightDir);
        }
    }
}


