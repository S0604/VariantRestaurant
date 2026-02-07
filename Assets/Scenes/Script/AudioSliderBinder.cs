using UnityEngine;
using UnityEngine.UI;

public class AudioSliderBinder : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private void OnEnable()
    {
        var mgr = AudioSettingsManager.Instance;
        if (mgr == null) return;

        // 先解除，避免重複綁定（很重要）
        if (masterSlider) masterSlider.onValueChanged.RemoveListener(mgr.SetMaster);
        if (bgmSlider) bgmSlider.onValueChanged.RemoveListener(mgr.SetBgm);
        if (sfxSlider) sfxSlider.onValueChanged.RemoveListener(mgr.SetSfx);

        // 回填目前值（不觸發事件）
        if (masterSlider) masterSlider.SetValueWithoutNotify(mgr.Master01);
        if (bgmSlider) bgmSlider.SetValueWithoutNotify(mgr.Bgm01);
        if (sfxSlider) sfxSlider.SetValueWithoutNotify(mgr.Sfx01);

        // 綁定事件：滑動就會呼叫 SetXxx
        if (masterSlider) masterSlider.onValueChanged.AddListener(mgr.SetMaster);
        if (bgmSlider) bgmSlider.onValueChanged.AddListener(mgr.SetBgm);
        if (sfxSlider) sfxSlider.onValueChanged.AddListener(mgr.SetSfx);
    }

    private void OnDisable()
    {
        var mgr = AudioSettingsManager.Instance;
        if (mgr == null) return;

        if (masterSlider) masterSlider.onValueChanged.RemoveListener(mgr.SetMaster);
        if (bgmSlider) bgmSlider.onValueChanged.RemoveListener(mgr.SetBgm);
        if (sfxSlider) sfxSlider.onValueChanged.RemoveListener(mgr.SetSfx);
    }
}
