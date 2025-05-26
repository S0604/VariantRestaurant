using UnityEngine;

public class DetectWall : MonoBehaviour
{
    public Transform player;         // 玩家物件
    public LayerMask wallLayer;     // 牆壁圖層
    private GameObject lastHitObj;  // 上次被射線擊中的物件

    void Update()
    {
        Vector3 dir = player.position - transform.position;
        float distance = dir.magnitude;
        RaycastHit hit;

        // 射線檢測
        if (Physics.Raycast(transform.position, dir.normalized, out hit, distance, wallLayer))
        {
            if (lastHitObj != hit.collider.gameObject)
            {
                // 恢復上次物件的不透明
                if (lastHitObj != null)
                    SetObjectTransparent(lastHitObj, false);

                // 讓這次物件變透明
                SetObjectTransparent(hit.collider.gameObject, true);
                lastHitObj = hit.collider.gameObject;
            }
        }
        else
        {
            // 沒有被阻擋，恢復物件不透明
            if (lastHitObj != null)
            {
                SetObjectTransparent(lastHitObj, false);
                lastHitObj = null;
            }
        }
    }

    void SetObjectTransparent(GameObject obj, bool isTransparent)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = renderer.material;
            Color color = mat.color;
            color.a = isTransparent ? 0.5f : 1f; // 可調整透明度
            mat.color = color;
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", isTransparent ? 0 : 1);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = isTransparent ? 3000 : 2000;
        }
    }
}
