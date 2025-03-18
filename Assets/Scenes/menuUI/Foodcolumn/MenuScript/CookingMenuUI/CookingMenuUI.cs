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
        if (ingredientStack.Count == 0)
            return BottomBunImage.rectTransform.anchoredPosition.y; // 没有食材时，酱料放在底部

        // 计算当前最上层食材的 Y 轴位置
        Image topImage = ingredientStack.Count == 1 ? BottomIngredientImage :
                         (ingredientStack.Count == 2 ? MiddleIngredientImage : TopIngredientImage);

        float topY = topImage.rectTransform.anchoredPosition.y;
        float sauceOffset = 170f; // 酱料稍微上移一点，避免与食材重叠

        return topY + sauceOffset;
    }

    private IEnumerator HandleSauceAnimation(IngredientData sauceData)
    {
        if (ingredientStack.Count == 0)
            yield break;

        Image movingImage = ingredientStack.Count == 1 ? MiddleIngredientImage :
                            (ingredientStack.Count == 2 ? TopIngredientImage : TopBunImage);

        Vector3 originalPosition = movingImage.rectTransform.anchoredPosition;
        Quaternion originalRotation = movingImage.rectTransform.rotation;

        Vector3 upPosition = originalPosition + new Vector3(0, 50, 0);
        Vector3 rightPosition = upPosition + new Vector3(200, 0, 0);
        Quaternion tiltRotation = Quaternion.Euler(0, 0, -15);

        float moveDuration = 0.5f;//上層麵包移動速度

        yield return StartCoroutine(MoveAndRotateUIElement(movingImage.rectTransform, upPosition, tiltRotation, moveDuration));
        yield return StartCoroutine(MoveAndRotateUIElement(movingImage.rectTransform, rightPosition, tiltRotation, moveDuration));

        ClearSauce();
        GameObject newSauce = Instantiate(sauceData.saucePrefab, stackPanel);
        newSauce.transform.SetSiblingIndex(GetSauceInsertIndex());
        spawnedSauces.Add(newSauce);

        // **修正 Y 轴**
        RectTransform sauceRect = newSauce.GetComponent<RectTransform>();
        sauceRect.anchoredPosition = new Vector2(sauceRect.anchoredPosition.x, GetSauceYOffset());

        Animator sauceAnimator = newSauce.GetComponent<Animator>();
        if (sauceAnimator != null)
        {
            sauceAnimator.CrossFade("ketchup", 0.1f);
            yield return new WaitForSeconds(sauceAnimator.GetCurrentAnimatorStateInfo(0).length);
        }

        yield return StartCoroutine(MoveAndRotateUIElement(movingImage.rectTransform, upPosition, originalRotation, moveDuration));
        yield return StartCoroutine(MoveAndRotateUIElement(movingImage.rectTransform, originalPosition, originalRotation, moveDuration));

        isSaucePlaying = false;
    }

    private IEnumerator MoveAndRotateUIElement(RectTransform rectTransform, Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        Vector3 startPosition = rectTransform.anchoredPosition;
        Quaternion startRotation = rectTransform.rotation;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);

            // 插值移动
            rectTransform.anchoredPosition = Vector3.Lerp(startPosition, targetPosition, progress);

            // 插值旋转
            rectTransform.rotation = Quaternion.Lerp(startRotation, targetRotation, progress);

            yield return null;
        }

        // 确保最终位置和角度正确
        rectTransform.anchoredPosition = targetPosition;
        rectTransform.rotation = targetRotation;
    }

}