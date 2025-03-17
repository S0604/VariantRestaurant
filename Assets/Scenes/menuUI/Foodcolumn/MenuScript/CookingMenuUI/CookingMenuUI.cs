using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;

public class CookingMenuUI : MonoBehaviour
{
    public static CookingMenuUI Instance;

    [SerializeField] private Transform stackPanel; // 在 Inspector 里手动赋值
    public Image TopBunImage;            // 面包顶部
    public Image TopIngredientImage;     // 对应 ItemSlot(2)
    public Image MiddleIngredientImage;  // 对应 ItemSlot(1)
    public Image BottomIngredientImage;  // 对应 ItemSlot(0)
    public Image BottomBunImage;         // 面包底部
    private List<GameObject> spawnedSauces = new List<GameObject>(); // 存放已生成的酱料对象
    private bool isSaucePlaying = false; // 防止重复调用
    public ItemSlot[] itemSlots; // 0:底部, 1:中部, 2:顶部, 3:基底格

    private List<IngredientData> ingredientStack = new List<IngredientData>(); // 存放当前食材

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning(" CookingMenuUI: 已有一个实例，可能存在多个对象！");
            Destroy(gameObject);
        }
    }

    public void CloseUI()
    {
        foreach (var slot in itemSlots)
        {
            slot.ClearSlot(); // 确保所有 ItemSlot 数据被清除
        }
        gameObject.SetActive(false);
    }

  

    private void ClearAllIngredientData()
    {
        foreach (var slot in itemSlots)
        {
            slot.ClearSlot();
            var hoverHandler = slot.GetComponent<ItemSlotHoverHandler>();
            hoverHandler?.ClearStoredIngredient();
        }

        foreach (GameObject sauce in spawnedSauces)
        {
            Destroy(sauce);
        }
        spawnedSauces.Clear();
    }

    public void UpdateStackPanel()
    {
        ingredientStack.Clear(); // 清空旧数据

        if (itemSlots.Length < 4)
        {
            Debug.LogError(" ItemSlots 数组长度不足！");
            return;
        }

        for (int i = 0; i < 3; i++) // 只处理 0~2（食材格）
        {
            IngredientData ingredient = itemSlots[i].GetStoredIngredient();
            if (ingredient != null)
            {
                ingredientStack.Add(ingredient);
            }
        }

        IngredientData bunIngredient = itemSlots[3].GetStoredIngredient(); // 基底格
        bool isBunBase = bunIngredient != null && bunIngredient.isBun;

        IngredientData bottomIngredient = ingredientStack.Count > 0 ? ingredientStack[0] : null;
        IngredientData middleIngredient = ingredientStack.Count > 1 ? ingredientStack[1] : null;
        IngredientData topIngredient = ingredientStack.Count > 2 ? ingredientStack[2] : null;

        UpdateIngredientImage(BottomIngredientImage, bottomIngredient);
        UpdateIngredientImage(MiddleIngredientImage, middleIngredient);
        UpdateIngredientImage(TopIngredientImage, topIngredient);

        bool topBunUsedAsFill = false;

        if (isBunBase)
        {
            if (ingredientStack.Count == 0)
            {
                BottomIngredientImage.sprite = bunIngredient.bunTopImage;
                BottomIngredientImage.enabled = true;
                topBunUsedAsFill = true;
            }
            else if (ingredientStack.Count == 1 && middleIngredient == null)
            {
                MiddleIngredientImage.sprite = bunIngredient.bunTopImage;
                MiddleIngredientImage.enabled = true;
                topBunUsedAsFill = true;
            }
            else if (ingredientStack.Count == 2 && topIngredient == null)
            {
                TopIngredientImage.sprite = bunIngredient.bunTopImage;
                TopIngredientImage.enabled = true;
                topBunUsedAsFill = true;
            }
        }

        if (topBunUsedAsFill)
        {
            TopBunImage.sprite = null;
            TopBunImage.enabled = false;
        }
        else if (isBunBase)
        {
            TopBunImage.sprite = bunIngredient.bunTopImage;
            TopBunImage.enabled = true;
        }

        if (isBunBase)
        {
            BottomBunImage.sprite = bunIngredient.bunBottomImage;
            BottomBunImage.enabled = true;
        }
        else
        {
            BottomBunImage.sprite = null;
            BottomBunImage.enabled = false;
        }
    }

    private void UpdateIngredientImage(Image image, IngredientData ingredient)
    {
        if (ingredient != null)
        {
            image.sprite = ingredient.ingredientImage;
            image.enabled = true;
        }
        else
        {
            image.sprite = default;
            image.enabled = false;
        }
    }

    public void SelectSauce(IngredientData sauceData)
    {
        if (sauceData == null || sauceData.saucePrefab == null)
        {
            Debug.LogWarning("⚠️ 选择的酱料无效或 saucePrefab 为空！");
            return;
        }

        if (isSaucePlaying)
        {
            Debug.Log("⏳ 酱料动画正在播放，等待完成...");
            return; // 防止多次点击时重复触发
        }

        isSaucePlaying = true; // 标记正在播放动画
        StartCoroutine(HandleSauceAnimation(sauceData));
    }
    private IEnumerator StopAnimationAfterPlay(Animator animator, string animationName)
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); // 等待动画播放完

        animator.Play(animationName, 0, 1); // **跳转到动画最后一帧**
        animator.speed = 0; // **停止动画**
    }

    private void ClearSauce()
    {
        foreach (GameObject sauce in spawnedSauces)
        {
            Destroy(sauce);
        }
        spawnedSauces.Clear();
    }


    private int GetSauceInsertIndex()
    {
        int ingredientCount = ingredientStack.Count;

        if (ingredientCount == 1)
        {
            return MiddleIngredientImage.transform.GetSiblingIndex();
        }
        else if (ingredientCount == 2)
        {
            return TopIngredientImage.transform.GetSiblingIndex();
        }
        else if (ingredientCount == 3)
        {
            return TopBunImage.transform.GetSiblingIndex();
        }

        return stackPanel.childCount;
    }
    private float GetSauceYOffset()
    {
        int ingredientCount = ingredientStack.Count;

        switch (ingredientCount)
        {
            case 0: return -30f;   // 没有食材，酱料稍微往下
            case 1: return -10f;  // 1 个食材，稍微上移
            case 2: return 10f;  // 2 个食材，接近顶部
            case 3: return 30f;  // 3 个食材，酱料放在最高层
            default: return 10f;  // 兜底情况
        }
    }
    private IEnumerator HandleSauceAnimation(IngredientData sauceData)
    {
        // 1️⃣ **移动 TopBunImage**
        Vector3 originalPosition = TopBunImage.rectTransform.anchoredPosition;
        Vector3 offsetPosition = originalPosition + new Vector3(200, 0, 0); // 向上移动 50 像素
        float moveDuration = 0.3f;

        yield return StartCoroutine(MoveUIElement(TopBunImage.rectTransform, offsetPosition, moveDuration));

        // 2️⃣ **清除旧的酱料**
        ClearSauce();

        // 3️⃣ **生成新的酱料**
        GameObject newSauce = Instantiate(sauceData.saucePrefab, stackPanel);
        newSauce.transform.SetSiblingIndex(GetSauceInsertIndex());
        spawnedSauces.Add(newSauce);

        // 4️⃣ **播放酱料动画**
        Animator sauceAnimator = newSauce.GetComponent<Animator>();
        if (sauceAnimator != null)
        {
            sauceAnimator.CrossFade("ketchup", 0.1f);
            yield return new WaitForSeconds(sauceAnimator.GetCurrentAnimatorStateInfo(0).length);
        }

        // 5️⃣ **TopBunImage 回归原位**
        yield return StartCoroutine(MoveUIElement(TopBunImage.rectTransform, originalPosition, moveDuration));

        isSaucePlaying = false; // 标记动画完成
    }
    private IEnumerator MoveUIElement(RectTransform rectTransform, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = rectTransform.anchoredPosition;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);
            rectTransform.anchoredPosition = Vector3.Lerp(startPosition, targetPosition, progress);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition; // 确保最终位置准确
    }

}