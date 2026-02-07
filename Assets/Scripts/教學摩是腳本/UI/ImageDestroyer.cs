using UnityEngine;
using UnityEngine.UI;

public class ImageDestroyer : MonoBehaviour
{
    public GameObject imageToDestroy; // 要銷毀的 Image

    public void DestroyImage()
    {
        if (imageToDestroy != null)
        {
            Destroy(imageToDestroy);
        }
    }
}
