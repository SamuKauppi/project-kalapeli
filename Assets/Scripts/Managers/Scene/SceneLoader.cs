using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton attached under PersitentManager
/// Used to switch scenes
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }
    [SerializeField] private SceneType[] scenes;
    private readonly Dictionary<SceneType, int> sceneDict = new();
    private bool isLoading = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        for (int i = 0; i < scenes.Length; i++)
        {
            sceneDict.Add(scenes[i], i);
        }
    }

    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        isLoading = true;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);

        MusicType type = sceneIndex == 0 ? MusicType.Menu : MusicType.Game1;
        LoopingSounds.Instance.SwitchMusic(type);
        LoopingSounds.Instance.SetGameMusicCoroutine(type == MusicType.Game1);

        // TODO: add loading screen
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        isLoading = false;
    }

    public void LoadScene(SceneType type)
    {
        if (!sceneDict.ContainsKey(type) || isLoading)
            return;

        StartCoroutine(LoadSceneAsync(sceneDict[type]));
    }

    public void LoadScene(int id)
    {
        if (isLoading)
            return;

        StartCoroutine(LoadSceneAsync(id));
    }
}
