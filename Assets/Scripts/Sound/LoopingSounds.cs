using System.Collections;
using UnityEngine;

public class LoopingSounds : MonoBehaviour
{
    public static LoopingSounds Instance { get; private set; }

    [SerializeField] private float musicTransitionTime = 0.5f;
    [SerializeField] private AudioSource menuMusicSource;
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource gameMusicSource;
    [SerializeField] private AudioSource gameMusicSource2;

    [SerializeField] private bool playGulls = true;
    [SerializeField] private float minTimeSeagulls;
    [SerializeField] private float maxTimeSeagulls;

    private float baseMenuVolume;
    private float baseGameVolume;
    private float baseGameVolume2;
    private float baseAmbientVolume;

    private float musicVolume = 1f;

    private MusicType currentType = MusicType.Menu;
    private Coroutine gameMusicRoutine;

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

        menuMusicSource.Play();
    }

    private void SetSourceVolumes()
    {
        if (baseMenuVolume <= 0f)
        {
            baseMenuVolume = menuMusicSource.volume;
        }

        if (baseGameVolume <= 0f)
        {
            baseGameVolume = gameMusicSource.volume;
        }

        if (baseGameVolume2 <= 0f)
        {
            baseGameVolume2 = gameMusicSource2.volume;
        }

        if (baseAmbientVolume <= 0f)
        {
            baseAmbientVolume = ambientSource.volume;
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

    private IEnumerator SwapMusic(AudioSource oldSource, float baseVolume1, AudioSource newSource, float baseVolume2)
    {
        float time = 0f;
        float transitionTime = musicTransitionTime * 0.5f;
        while (time < transitionTime)
        {
            oldSource.volume = Mathf.Lerp(baseVolume1 * musicVolume, 0f, time / transitionTime);
            time += Time.deltaTime;
            yield return null;
        }
        oldSource.volume = 0f;
        oldSource.Stop();
        newSource.Play();
        time = 0f;

        while (time < transitionTime)
        {
            newSource.volume = Mathf.Lerp(0f, baseVolume2 * musicVolume, time / transitionTime);
            time += Time.deltaTime;
            yield return null;
        }
        newSource.volume = baseVolume2;
    }

    private void GetSourceVolume(MusicType type, out float volume1, out AudioSource audio1)
    {
        switch (type)
        {
            case MusicType.Menu:
                audio1 = menuMusicSource;
                volume1 = baseMenuVolume;
                break;
            case MusicType.Game1:
                audio1 = gameMusicSource;
                volume1 = baseGameVolume;
                break;
            case MusicType.Game2:
                audio1 = gameMusicSource2;
                volume1 = baseGameVolume2;
                break;
            default:
                audio1 = menuMusicSource;
                volume1 = baseMenuVolume;
                break;
        }
    }

    private IEnumerator GameMusicSwapping()
    {
        while (true)
        {
            AudioSource source = currentType == MusicType.Game1 ? gameMusicSource : gameMusicSource2;
            yield return new WaitForSeconds(source.clip.length - source.time);
            SwitchMusic(currentType == MusicType.Game1 ? MusicType.Game2 : MusicType.Game1);
        }
    }

    public void ChangeVolume(SoundClipType type, float volume)
    {
        // Base volume has to be set this way since this is called before start
        SetSourceVolumes();

        if (type == SoundClipType.MainAmbience && ambientSource != null)
        {
            ambientSource.volume = volume * baseAmbientVolume;
        }

        if (type == SoundClipType.Music)
        {
            musicVolume = volume;
            menuMusicSource.volume = volume * baseMenuVolume;
            gameMusicSource.volume = volume * baseGameVolume;
            gameMusicSource2.volume = volume * baseGameVolume2;
        }
    }

    public void SwitchMusic(MusicType type)
    {
        GetSourceVolume(currentType, out float oldVolume, out AudioSource oldSource);
        GetSourceVolume(type, out float newVolume, out AudioSource newSource);

        if (oldSource != newSource)
        {
            StartCoroutine(SwapMusic(oldSource, oldVolume, newSource, newVolume));
        }

        currentType = type;
    }

    public void SetGameMusicCoroutine(bool value)
    {
        if (value && gameMusicRoutine == null)
        {
            gameMusicRoutine = StartCoroutine(GameMusicSwapping());
        }
        else if (gameMusicRoutine != null)
        {
            StopCoroutine(gameMusicRoutine);
            gameMusicRoutine = null;
        }
    }
}

public enum MusicType
{
    Menu,
    Game1,
    Game2
}