using UnityEngine;
using UnityEngine.UI;

public class SettingsPage : MonoBehaviour
{
    public static SettingsPage Instance { get; private set; }

    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider ambientSlider;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        musicSlider.value = PlayerPrefManager.Instance.GetPrefValue(SaveValue.music_volume, 1f);
        sfxSlider.value = PlayerPrefManager.Instance.GetPrefValue(SaveValue.sfx_volume, 1f);
        ambientSlider.value = PlayerPrefManager.Instance.GetPrefValue(SaveValue.ambient_volume, 1f);
    }


    public void ChangeMusicVolume(float value)
    {
        PlayerPrefManager.Instance.SavePrefValue(SaveValue.music_volume, value);

        if (LoopingSounds.Instance != null)
            LoopingSounds.Instance.ChangeVolume(SoundClipType.Music, value);
    }

    public void ChangeSoundVolume(float value)
    {
        PlayerPrefManager.Instance.SavePrefValue(SaveValue.sfx_volume, value);

        if (SoundManager.Instance != null)
            SoundManager.Instance.ChangeVolume(value);
    }

    public void ChangeAmbientVolume(float value)
    {
        PlayerPrefManager.Instance.SavePrefValue(SaveValue.ambient_volume, value);

        if (LoopingSounds.Instance != null)
            LoopingSounds.Instance.ChangeVolume(SoundClipType.MainAmbience, value);
    }
}
