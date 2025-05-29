using UnityEngine;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance;

    private AudioSource audioSource;

    [Header("UI ���İſ�")]
    public AudioClip hoverClip;
    public AudioClip clickClip;

    [Range(0f, 1f)]
    public float volume = 1f;

    private void Awake()
    {
        // ����ƹ��
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ���ը��o AudioSource�A�Φ۰ʲK�[
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
        // �ʺA��s���q�]�i�̻ݨD�R���^
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
            Debug.LogWarning("�����w AudioClip �� AudioSource �ʥ�");
        }
    }
}
