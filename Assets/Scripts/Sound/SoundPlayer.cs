using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private SoundClipType[] soundTypes;
    [SerializeField] private SoundClipTrigger trigger;
    [SerializeField] private SoundClipPlayOrder playOrder;

    private AudioClip[] clips;
    private int clipIndex = 0;
    private int playIndex;

    public SoundClipType[] SoundTypes { get { return soundTypes; } }
    public SoundClipTrigger Trigger { get { return trigger; } }

    private void Start()
    {
        clips = new AudioClip[soundTypes.Length];
    }

    public void AddSoundClip(AudioClip clip)
    {
        clips[playIndex] = clip;
        clipIndex = (clipIndex + 1) % clips.Length;
    }

    public void PlaySound()
    {
        int index = playOrder == SoundClipPlayOrder.Random ? Random.Range(0, clips.Length) : playIndex % clips.Length;
        source.clip = clips[index];
        source.Play();
        playIndex++;
    }
}
