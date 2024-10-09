using UnityEngine;

/// <summary>
/// Manages fishing portion of the game
/// </summary>
public class FishManager : MonoBehaviour
{
    // Fishes for this level
    private FishInstance[] availableFish;

    private void Start()
    {
        availableFish = PersitentManager.Instance.GetFishesForThisLevel();
    }
}