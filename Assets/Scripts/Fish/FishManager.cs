using UnityEngine;

/// <summary>
/// Manages fishing portion of the game
/// </summary>
public class FishManager : MonoBehaviour
{
    public static FishManager Instance { get; private set; }

    // Fishes for this level
    [SerializeField] private Fish[] availableFish;
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
    }

    private void Start()
    {
        availableFish = PersitentManager.Instance.GetFishesForThisLevel();
    }

    public void EndFishing()
    {
        GameManager.Instance.SwapModes();
    }
}
