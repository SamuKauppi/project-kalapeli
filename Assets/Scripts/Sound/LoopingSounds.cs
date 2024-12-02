using System.Collections;
using UnityEngine;

public class LoopingSounds : MonoBehaviour
{
    public static LoopingSounds Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambientSource;

    [SerializeField] private bool playGulls = true;
    [SerializeField] private float minTimeSeagulls;
    [SerializeField] private float maxTimeSeagulls;

    private float baseMusicVolume = 0f;
    private float baseAmbientVolume = 0f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (playGulls)
            StartCoroutine(PlayRandomAmbient());
    }

    public void ChangeVolume(SoundClipType type, float volume)
    {
        if (type == SoundClipType.MainAmbience && ambientSource != null)
        {
            // Base volume has to be set way since this is called before start
            if (baseAmbientVolume <= 0f)
            {
                baseAmbientVolume = ambientSource.volume;
            }
            ambientSource.volume = volume * baseAmbientVolume;
        }

        if (type == SoundClipType.Music && musicSource != null)
        {
            // Base volume has to be set this way since this is called before start
            if (baseMusicVolume <= 0f)
            {
                baseMusicVolume = musicSource.volume;
            }

            musicSource.volume = volume * baseMusicVolume;
        }
    }

    private IEnumerator PlayRandomAmbient()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minTimeSeagulls, maxTimeSeagulls));
            SoundManager.Instance.PlaySound(SoundClipTrigger.OnSeagulls);
        }
    }
}
