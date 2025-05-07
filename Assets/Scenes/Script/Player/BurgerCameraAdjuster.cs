using UnityEngine;

public class BurgerCameraAdjuster : MonoBehaviour
{
    public Transform targetTransform;      // 被攝影機鎖定的目標（通常是漢堡中心空物件）
    public float offsetPerLayer = 120f;    // 每層食材的高度差
    public float extraYOffset = 0f;        // 額外手動調整的偏移量（可調整拍攝構圖）

    private Vector3 originalPosition;      // 儲存原始位置以便回復

    // 調整目標位置並提供還原方法
    public void AdjustTargetY(int ingredientCount)
    {
        // 儲存初始位置
        originalPosition = targetTransform.position;

        // 計算新的 Y 值
        float offsetY = (ingredientCount * offsetPerLayer) + extraYOffset;
        Vector3 newPosition = originalPosition;
        newPosition.y += offsetY;

        // 設定新的位置
        targetTransform.position = newPosition;

        Debug.Log($"[BurgerCameraAdjuster] 調整目標 Y 軸：原始 {originalPosition.y} → 調整後 {newPosition.y}（層數 {ingredientCount}）");
    }

    // 回復原始位置
    public void ResetTargetY()
    {
        targetTransform.position = originalPosition;
        Debug.Log($"[BurgerCameraAdjuster] 已回復目標位置為：{originalPosition}");
    }
}
