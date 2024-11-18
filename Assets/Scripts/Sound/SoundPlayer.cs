﻿using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private SoundClipType[] soundTypes;
    [SerializeField] private SoundClipTrigger trigger;
    [SerializeField] private SoundClipPlayOrder playOrder;

    private AudioClip[] clips;
    private int playIndex;

    public SoundClipType[] SoundTypes { get { return soundTypes; } }
    public SoundClipTrigger Trigger { get { return trigger; } }

    private void Start()
    {
        clips = new AudioClip[soundTypes.Length];
        for (int i = 0; i < clips.Length; i++)
        {
            clips[i] = SoundManager.Instance.GetClip(soundTypes[i]);
        }
    }

    private void OnEnable()
    {
        SoundManager.OnPlaySound += PlaySound;
    }

    private void OnDisable()
    {
        SoundManager.OnPlaySound -= PlaySound;
    }

    public void PlaySound(SoundClipTrigger trigger)
    {
        if (trigger == Trigger)
        {
            int index = playOrder == SoundClipPlayOrder.Random ? Random.Range(0, clips.Length) : playIndex % clips.Length;
            source.clip = clips[index];
            source.Play();
            playIndex++;
        }
    }
}