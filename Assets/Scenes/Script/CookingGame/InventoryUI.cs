using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Prefab & Container")]
    public GameObject itemImagePrefab;   // 建議是單純含 Image 的 prefab
    public Transform itemContainer;

    [Header("Display Box (UI 尺寸，不改圖片解析度)")]
    [Tooltip("每個物品槽位在 UI 上的固定寬高（像素）")]
    public Vector2 itemBoxSize = new Vector2(128f, 128f);

    [Tooltip("是否強制使用固定顯示尺寸（關閉則沿用 prefab 尺寸）")]
    public bool useFixedDisplaySize = true;

    [Header("Bottom Align")]
    [Tooltip("是否讓圖片在槽位中：等比縮放並『底部置中』對齊")]
    public bool bottomAlign = true;

    [Header("Sprite Source")]
    [Tooltip("勾選後：此 UI 顯示時優先使用 MenuItem 的 grade 對應圖（gradeSprites）；取消則直接用 itemImage")]
    public bool useGradeSpriteInUI = true;

    [Header("Layout（未使用 LayoutGroup 時生效）")]
    [Tooltip("未使用 LayoutGroup 時，兩個槽位之間的水平間距")]
    public float slotSpacing = 100f;

    private readonly List<GameObject> spawnedSlots = new List<GameObject>();

    private void OnEnable()
    {
        InventoryManager.OnInventoryChanged += UpdateUI;
    }

    private void OnDisable()
    {
        InventoryManager.OnInventoryChanged -= UpdateUI;
    }

    public void UpdateUI(List<MenuItem> items)
    {
        if (itemImagePrefab == null || itemContainer == null)
        {
            Debug.LogError("Prefab 或 Container 沒有設定！");
            return;
        }

        // 清掉舊的
        foreach (var go in spawnedSlots) Destroy(go);
        spawnedSlots.Clear();

        // 過濾可視項（排除 SupplyBox）
        List<MenuItem> visibleItems = items.FindAll(item => item.itemTag != "SupplyBox");

        int showCount = Mathf.Min(2, visibleItems.Count);
        for (int i = 0; i < showCount; i++)
        {
            var item = visibleItems[i];

            // --- 建立「槽位容器」：固定 UI 尺寸 ---
            GameObject slot = new GameObject($"Slot_{i}", typeof(RectTransform));
            var slotRT = slot.GetComponent<RectTransform>();
            slot.transform.SetParent(itemContainer, false);

            // 如果沒有使用 LayoutGroup，就手動擺位置
            slotRT.anchorMin = new Vector2(0.5f, 0.5f);
            slotRT.anchorMax = new Vector2(0.5f, 0.5f);
            slotRT.pivot = new Vector2(0.5f, 0.5f);

            if (useFixedDisplaySize)
            {
                slotRT.sizeDelta = itemBoxSize;

                // 若父物件有 LayoutGroup，鎖定尺寸避免被覆寫
                var le = slot.AddComponent<LayoutElement>();
                le.preferredWidth = itemBoxSize.x;
                le.preferredHeight = itemBoxSize.y;
                le.minWidth = itemBoxSize.x;
                le.minHeight = itemBoxSize.y;
                le.flexibleWidth = 0;
                le.flexibleHeight = 0;
            }

            // 如果沒用 LayoutGroup，採用簡單水平排版
            slotRT.anchoredPosition = new Vector2(i * slotSpacing, 0f);

            // --- 建立圖片物件（作為槽位的子物件） ---
            GameObject imgObj = Instantiate(itemImagePrefab);
            imgObj.name = $"ItemImage_{i}";
            imgObj.transform.SetParent(slot.transform, false);

            var img = imgObj.GetComponent<Image>();
            var imgRT = imgObj.GetComponent<RectTransform>();
            if (img == null || imgRT == null)
            {
                Debug.LogError("itemImagePrefab 需要有 Image + RectTransform。");
                Destroy(slot);
                continue;
            }

            // === 選擇要顯示的圖：grade 對應圖（優先）或 itemImage（退回） ===
            Sprite spriteToShow = null;
            if (useGradeSpriteInUI && item != null)
            {
                spriteToShow = item.GetSpriteByGrade(item.grade);
            }
            if (spriteToShow == null && item != null)
            {
                spriteToShow = item.itemImage; // 退回既有 itemImage，不動到別系統
            }

            if (spriteToShow == null)
            {
                img.sprite = null;
                img.color = new Color(1, 1, 1, 0); // 隱藏
            }
            else
            {
                img.sprite = spriteToShow;
                img.color = Color.white;
            }

            // --- 對齊與尺寸：底部置中 + 等比縮放以「Fit 到槽位」 ---
            if (bottomAlign)
            {
                // 底部置中對齊（以槽位為參考）
                imgRT.anchorMin = new Vector2(0.5f, 0f);
                imgRT.anchorMax = new Vector2(0.5f, 0f);
                imgRT.pivot = new Vector2(0.5f, 0f);
                imgRT.anchoredPosition = Vector2.zero;

                // 等比縮放到不超過槽位尺寸（Fit），計算像素尺寸
                Vector2 boxSize = useFixedDisplaySize ? itemBoxSize : slotRT.rect.size;
                if (img.sprite != null)
                {
                    float sw = img.sprite.rect.width;
                    float sh = img.sprite.rect.height;
                    if (sw > 0f && sh > 0f)
                    {
                        float scale = Mathf.Min(boxSize.x / sw, boxSize.y / sh);
                        float w = Mathf.Round(sw * scale);
                        float h = Mathf.Round(sh * scale);
                        imgRT.sizeDelta = new Vector2(w, h);
                    }
                    else
                    {
                        imgRT.sizeDelta = boxSize;
                    }
                }
                else
                {
                    imgRT.sizeDelta = boxSize;
                }

                img.type = Image.Type.Simple;
                img.preserveAspect = false; // 我們手動算過尺寸了
            }
            else
            {
                // 不啟用 bottomAlign：置中 + 等比 Fit
                imgRT.anchorMin = imgRT.anchorMax = imgRT.pivot = new Vector2(0.5f, 0.5f);
                imgRT.anchoredPosition = Vector2.zero;

                Vector2 boxSize = useFixedDisplaySize ? itemBoxSize : slotRT.rect.size;
                if (img.sprite != null)
                {
                    float sw = img.sprite.rect.width;
                    float sh = img.sprite.rect.height;
                    if (sw > 0f && sh > 0f)
                    {
                        float scale = Mathf.Min(boxSize.x / sw, boxSize.y / sh);
                        imgRT.sizeDelta = new Vector2(Mathf.Round(sw * scale), Mathf.Round(sh * scale));
                    }
                    else imgRT.sizeDelta = boxSize;
                }
                else imgRT.sizeDelta = boxSize;

                img.type = Image.Type.Simple;
                img.preserveAspect = false;
            }

            spawnedSlots.Add(slot);
        }

        Debug.Log($"更新 UI：顯示 {showCount} 個可視物品（底部對齊：{bottomAlign}，用 gradeSprite：{useGradeSpriteInUI}）");
    }
}
