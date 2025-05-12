using System.Collections.Generic;
using UnityEngine;

public class UIOrderDisplayPool : MonoBehaviour
{
    public GameObject oneSlotPrefab;
    public GameObject twoSlotPrefab;

    private Queue<GameObject> oneSlotPool = new();
    private Queue<GameObject> twoSlotPool = new();

    public GameObject Get(bool isOneSlot, Transform parent)
    {
        var pool = isOneSlot ? oneSlotPool : twoSlotPool;
        var prefab = isOneSlot ? oneSlotPrefab : twoSlotPrefab;

        GameObject go = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab);
        go.transform.SetParent(parent, false);
        go.SetActive(true);
        return go;
    }

    public void Return(GameObject obj, bool isOneSlot)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform); // 收進 pool 物件下方

        var pool = isOneSlot ? oneSlotPool : twoSlotPool;
        pool.Enqueue(obj);
    }
}
