using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverSound : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public AudioClip hoverSound;
    [Range(0f, 2f)] public float volume = 1.0f;

    private bool hasPlayed = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!hasPlayed && hoverSound != null)
        {
            UIAudioManager.Instance?.PlaySound(hoverSound);
            hasPlayed = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hasPlayed = false;
    }
}
