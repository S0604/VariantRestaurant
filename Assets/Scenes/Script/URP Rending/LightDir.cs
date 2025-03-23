using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Light : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Vector3 LightDir = -mainLight.transform.forward * -1;
        
    }

    public class LightDirectionSetter : MonoBehaviour
{
    public Light mainLight;
    public Material targetMaterial;

    void Update()
    {
        if (mainLight != null && targetMaterial != null)
        {
            Vector3 lightDir = -mainLight.transform.forward;
            targetMaterial.SetVector("_LightDir", lightDir);
             }

    // Update is called once per frame
    void Update()
    {
        
    }
}
