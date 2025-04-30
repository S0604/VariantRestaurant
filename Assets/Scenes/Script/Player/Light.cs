using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Light : MonoBehaviour
{
    public Light mainLight; // ָ������Դ��Directional Light��
    public Material targetMaterial; // ָ��ҪӰ푵Ĳ��|

    void Update()
    {
        if (mainLight != null && targetMaterial != null)
        {
            // ȡ�ù��շ���Unity �� Directional Light �� forward �����c���շ����෴��
            Vector3 lightDir = -mainLight.transform.forward;

            // �����շ�����f�o Shader
            targetMaterial.SetVector("_LightDir", lightDir);
        }
    }
}


