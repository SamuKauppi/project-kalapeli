using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles sliders that update settings
/// Also displays secret
/// </summary>
public class SettingsPage : MonoBehaviour
{
    public static SettingsPage Instance { get; private set; }

    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider ambientSlider;
    [SerializeField] private GameObject completeButton;
    [SerializeField] private GameObject resetPopUp;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        if (completeButton && Input.GetKey(KeyCode.Alpha6) && Input.GetKeyDown(KeyCode.Alpha9))
        {
            completeButton.SetActive(true);
            SavingManager.Instance.CatchEveryFish();
            FishCatalog.Instance.ShowCatalog(0);
        }
    }

    private void Start()
    {
        musicSlider.value = PlayerPrefManager.Instance.GetPrefValue(SaveValue.music_volume, 1f);
        LoopingSounds.Instance.ChangeVolume(SoundClipType.Music, musicSlider.value);

        sfxSlider.value = PlayerPrefManager.Instance.GetPrefValue(SaveValue.sfx_volume, 1f);
        SoundManager.Instance.ChangeVolume(sfxSlider.value);

        ambientSlider.value = PlayerPrefManager.Instance.GetPrefValue(SaveValue.ambient_volume, 1f);
        LoopingSounds.Instance.ChangeVolume(SoundClipType.MainAmbience, ambientSlider.value);

        resetPopUp.SetActive(false);

        if (completeButton)
            completeButton.SetActive(PlayerPrefManager.Instance.GetPrefValue(SaveValue.game_Complete, 0) != 0);
    }

    public void CheckForGameComplete()
    {
        foreach (FishSpecies fish in Enum.GetValues(typeof(FishSpecies)))
        {
            if (fish != FishSpecies.None && !PersitentManager.Instance.IsFishCaught(fish))
            {
                return;
            }
        }
        PlayerPrefManager.Instance.SavePrefValue(SaveValue.game_Complete, 1);
        if (completeButton)
            completeButton.SetActive(true);
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

    public void ResetSaves(bool isFirstTime)
    {
        if (isFirstTime)
        {
            resetPopUp.SetActive(true);
            return;
        }
        resetPopUp.SetActive(false);
        SavingManager.Instance.ResetSaveData();
        FishCatalog.Instance.ShowCatalog(0);
        FishCatalog.Instance.DisplayFish(FishSpecies.None);
        if (completeButton)
            completeButton.SetActive(false);
    }
}
