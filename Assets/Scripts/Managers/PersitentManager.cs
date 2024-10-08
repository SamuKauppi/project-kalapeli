using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main singleton used to transfer data between scenes
/// </summary>
public class PersitentManager : MonoBehaviour
{
    public static PersitentManager Instance { get; private set; }
    public List<GameObject> lureProperties = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void AddLure(GameObject lureObj)
    {
        lureObj.SetActive(false);
        GameObject lure = Instantiate(lureObj, transform);

        if (lure.TryGetComponent<BlockRotation>(out var rot))
        {
            Destroy(rot);
        }
        lureProperties.Add(lure);
        SceneLoader.Instance.LoadScene(SceneType.Fishing);
        lureObj.SetActive(true);
    }
}
