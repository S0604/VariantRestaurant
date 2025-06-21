using UnityEngine;
using System.Collections.Generic;

public class DetectWall : MonoBehaviour
{
    public Transform player;         // 玩家物件
    public LayerMask wallLayer;     // 牆壁圖層（遮擋物）
    private List<GameObject> transparentObjects = new List<GameObject>(); // 透明物件清單

    void Update()
    {
        Vector3 dir = player.position - transform.position;
        float distance = dir.magnitude;

        // 射線檢測（偵測多層遮擋）
        RaycastHit[] hits = Physics.RaycastAll(transform.position, dir.normalized, distance, wallLayer);
        bool playerVisible = hits.Length == 0;

        // 若有遮擋物，將它們設為透明
        if (!playerVisible)
        {
            foreach (RaycastHit h in hits)
            {
                GameObject obj = h.collider.gameObject;
                if (!transparentObjects.Contains(obj))
                {
                    SetObjectTransparent(obj, true);
                    transparentObjects.Add(obj);
                }
            }
        }

        // 若玩家可見，恢復所有透明物件
        if (playerVisible)
        {
            foreach (GameObject obj in transparentObjects)
            {
                SetObjectTransparent(obj, false);
            }
            transparentObjects.Clear();
        }
    }

    void SetObjectTransparent(GameObject obj, bool isTransparent)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null) return;

        Material mat = new Material(renderer.material); // 產生新材質，避免共用材質影響其他物件
        Color color = mat.color;
        color.a = isTransparent ? 0.3f : 1f;
        mat.color = color;

        // 設定材質為透明模式
        if (isTransparent)
        {
            mat.SetFloat("_Mode", 3); // 3=Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }
        else
        {
            mat.SetFloat("_Mode", 0); // 0=Opaque
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 2000;
        }
        renderer.material = mat;
    }
}
