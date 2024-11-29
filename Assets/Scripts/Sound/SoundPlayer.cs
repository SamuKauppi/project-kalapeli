using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float volume;
    [SerializeField] private SoundClipType[] soundTypes;
    [SerializeField] private SoundClipTrigger trigger;
    [SerializeField] private SoundClipPlayOrder playOrder;

    private readonly Queue<AudioSource> audioSources = new();
    private readonly int maxAudioSources = 3;
    private AudioClip[] clips;
    private int playIndex;

    private float baseVolume;

    public SoundClipType[] SoundTypes { get { return soundTypes; } }
    public SoundClipTrigger Trigger { get { return trigger; } }

    private void Start()
    {
        for (int i = 0; i < maxAudioSources; i++)
        {
            var newSource = gameObject.AddComponent<AudioSource>();
            audioSources.Enqueue(newSource);
        }

        clips = new AudioClip[soundTypes.Length];
        for (int i = 0; i < clips.Length; i++)
        {
            clips[i] = SoundManager.Instance.GetClip(soundTypes[i]);
        }
        baseVolume = volume;
    }

    private void OnEnable()
    {
        SoundManager.OnPlaySound += PlaySound;
    }

    private void OnDisable()
    {
        SoundManager.OnPlaySound -= PlaySound;
    }

    public void PlaySound(SoundClipTrigger trigger, float volumeMultiplier)
    {
        if (trigger == Trigger)
        {
            AudioSource source = audioSources.Dequeue();
            audioSources.Enqueue(source);

            if (source.isPlaying)
                source.Stop();

            source.volume = baseVolume * volumeMultiplier;
            int index = playOrder == SoundClipPlayOrder.Random ? Random.Range(0, clips.Length) : playIndex % clips.Length;
            source.clip = clips[index];
            source.Play();
            playIndex++;
        }
    }
}
