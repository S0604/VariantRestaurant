using UnityEngine;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance;

    private AudioSource audioSource;

    [Header("UI 音效剪輯")]
    public AudioClip hoverClip;
    public AudioClip clickClip;

    [Range(0f, 1f)]
    public float volume = 1f;

    private void Awake()
    {
        // 防止重複實例
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 嘗試取得 AudioSource，或自動添加
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.volume = volume;
    }

    private void Update()
    {
        // 動態更新音量（可依需求刪除）
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    public void PlayHoverSound()
    {
        PlaySound(hoverClip);
    }

    public void PlayClickSound()
    {
        PlaySound(clickClip);
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("未指定 AudioClip 或 AudioSource 缺失");
        }
    }
}
