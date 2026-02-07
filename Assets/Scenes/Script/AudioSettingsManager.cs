using UnityEngine;
using UnityEngine.Audio;

public class AudioSettingsManager : MonoBehaviour
{
    public static AudioSettingsManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mixer;

    [Header("Exposed parameter names")]
    [SerializeField] private string masterParam = "MasterVol";
    [SerializeField] private string bgmParam = "BGMVol";
    [SerializeField] private string sfxParam = "SFXVol";

    // PlayerPrefs keys
    private const string PREF_MASTER = "audio_master_01";
    private const string PREF_BGM = "audio_bgm_01";
    private const string PREF_SFX = "audio_sfx_01";

    private const float MIN_DB = -80f;

    public float Master01 { get; private set; } = 1f;
    public float Bgm01 { get; private set; } = 1f;
    public float Sfx01 { get; private set; } = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAndApply();
    }

    public void SetMaster(float value01)
    {
        Master01 = Mathf.Clamp01(value01);
        Apply(masterParam, Master01);
        PlayerPrefs.SetFloat(PREF_MASTER, Master01);
    }

    public void SetBgm(float value01)
    {
        Bgm01 = Mathf.Clamp01(value01);
        Apply(bgmParam, Bgm01);
        PlayerPrefs.SetFloat(PREF_BGM, Bgm01);
    }

    public void SetSfx(float value01)
    {
        Sfx01 = Mathf.Clamp01(value01);
        Apply(sfxParam, Sfx01);
        PlayerPrefs.SetFloat(PREF_SFX, Sfx01);
    }

    public void SaveNow() => PlayerPrefs.Save();

    private void LoadAndApply()
    {
        Master01 = PlayerPrefs.GetFloat(PREF_MASTER, 1f);
        Bgm01 = PlayerPrefs.GetFloat(PREF_BGM, 1f);
        Sfx01 = PlayerPrefs.GetFloat(PREF_SFX, 1f);

        Apply(masterParam, Master01);
        Apply(bgmParam, Bgm01);
        Apply(sfxParam, Sfx01);
    }

    private void Apply(string paramName, float value01)
    {
        if (mixer == null || string.IsNullOrEmpty(paramName)) return;

        float db = Linear01ToDb(value01);
        mixer.SetFloat(paramName, db);
    }

    private float Linear01ToDb(float value01)
    {
        if (value01 <= 0.0001f) return MIN_DB;
        float db = 20f * Mathf.Log10(value01); // 0~1 -> dB
        return Mathf.Max(db, MIN_DB);
    }
}
