using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Singleton attached under PersitentManager
/// Used to switch scenes
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }
    [SerializeField] private SceneType[] scenes;
    [SerializeField] private Animator loadAnim;
    [SerializeField] private float animationTime = 1f;
    [SerializeField] private float animationSpeed = 1f;
    [SerializeField] private Slider loadingBar;
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
    private void Start()
    {
        loadAnim.speed = animationSpeed;
        loadingBar.value = 0f;
    }

    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        loadAnim.SetBool("IsLoading", true);
        yield return new WaitForSeconds(animationTime * animationSpeed);

        isLoading = true;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);

        MusicType type = sceneIndex == 0 ? MusicType.Menu : MusicType.Game1;
        LoopingSounds.Instance.SwitchMusic(type);
        LoopingSounds.Instance.SetGameMusicCoroutine(type == MusicType.Game1);

        while (!asyncLoad.isDone)
        {
            loadingBar.value = asyncLoad.progress;
            yield return null;
        }
        isLoading = false;
        loadAnim.SetBool("IsLoading", false);
        loadingBar.value = 1f;
    }

    public void LoadScene(SceneType type)
    {
        if (!sceneDict.ContainsKey(type) || isLoading)
            return;
        loadingBar.value = 0f;
        StartCoroutine(LoadSceneAsync(sceneDict[type]));
    }

    public void LoadScene(int id)
    {
        if (isLoading)
            return;
        loadingBar.value = 0f;
        StartCoroutine(LoadSceneAsync(id));
    }
}
