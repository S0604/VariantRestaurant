using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chapter : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        TutorialProgressManager.Instance.CompleteEvent("MobileTeachingCompleted");
   
        FindObjectOfType<TutorialDialogueController>().PlayChapter("4");
        Destroy(gameObject);   // 只觸發一次
    }
}
