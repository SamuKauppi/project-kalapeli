using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private SoundClipType[] types;
    [SerializeField] private AudioClip[] clips;

    private readonly Dictionary<SoundClipType, AudioClip> soundDict = new();

    public delegate void PlaySoundEvent(SoundClipTrigger type, float volume);
    public static event PlaySoundEvent OnPlaySound;

    private float volumeMultiplier;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        for (int i = 0; i < types.Length; i++)
        {
            soundDict.Add(types[i], clips[i]);
        }
        volumeMultiplier = PlayerPrefManager.Instance.GetPrefValue(SaveValue.sfx_volume, 1f);
    }

    public AudioClip GetClip(SoundClipType type)
    {
        return soundDict[type];
    }

    public void PlaySound(SoundClipTrigger triggerType)
    {
        OnPlaySound?.Invoke(triggerType, volumeMultiplier);
    }

    public void ChangeVolume(float volume)
    {
        volumeMultiplier = volume;
    }
}
