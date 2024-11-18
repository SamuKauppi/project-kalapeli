using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private SoundClipPair[] sounds;
    private readonly Dictionary<SoundClipType, AudioClip> soundDict = new();

    public delegate void PlaySoundEvent(SoundClipTrigger type);
    public static event PlaySoundEvent OnPlaySound;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    private void Start()
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            soundDict.Add(sounds[i].type, sounds[i].clip);
        }
    }

    public AudioClip GetClip(SoundClipType type)
    {
        return soundDict[type];
    }

    public void PlaySound(SoundClipTrigger triggerType)
    {
        OnPlaySound?.Invoke(triggerType);
    }
}
