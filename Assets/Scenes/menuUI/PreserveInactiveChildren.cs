using UnityEngine;
using System.Collections.Generic;

public class PreserveInactiveChildren : MonoBehaviour
{
    private void OnDisable()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}