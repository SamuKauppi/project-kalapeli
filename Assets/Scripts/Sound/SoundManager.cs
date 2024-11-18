using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private SoundClipPair[] sounds;
    private readonly Dictionary<SoundClipType, AudioClip> soundDict = new();

    private SoundPlayer[] soundPlayers;

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

        soundPlayers = new SoundPlayer[transform.childCount];
        for (int i = 0; i < soundPlayers.Length; i++)
        {
            SoundClipType[] clipTypes = soundPlayers[i].SoundTypes;
            for (int j = 0; j < clipTypes.Length; j++)
            {
                soundPlayers[i].AddSoundClip(soundDict[clipTypes[j]]);
            }
        }
    }

    public void PlaySounds(SoundClipTrigger triggerType)
    {
        foreach (SoundPlayer player in soundPlayers)
        {
            if (player.Trigger == triggerType)
            {
                player.PlaySound();
            }
        }
    }
}
