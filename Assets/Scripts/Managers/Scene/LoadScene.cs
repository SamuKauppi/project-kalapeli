using UnityEngine;

public class LoadScene : MonoBehaviour
{
    [SerializeField] private SceneType sceneToLoad;

    public void StartLoad()
    {
        SceneLoader.Instance.LoadScene(sceneToLoad);
    }
}
