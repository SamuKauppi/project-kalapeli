using System.Collections;
using UnityEngine;

/// <summary>
/// Used to catch fishes
/// </summary>
public class Rod : MonoBehaviour
{
    public bool IsAttached { get; private set; } = false;                   // If a lure is attached
    public LureStats LureAttached { get; private set; } = null;             // Lure attached
    public bool HasFish { get; private set; } = false;                      // Has a fish attached
    public FishSpecies CaughtFish { get; private set; } = FishSpecies.None; // Fish species attached

    // Reference to outline script
    [SerializeField] private Outline outline;     
    // Min & Max time to wait for fish to be caught
    [SerializeField] private float minTimeForFish;
    [SerializeField] private float maxTimeForFish;

    // Got from FishManager
    private FishCatchScore[] fishCatchChances;

    // Total catchscore
    private int totalCatchScore;

    private void Start()
    {
        outline.enabled = false;
    }

    private void OnMouseEnter()
    {
        if (!IsAttached || HasFish)
            outline.enabled = true;
    }

    private void OnMouseExit()
    {
        if (!IsAttached || HasFish)
            outline.enabled = false;
    }

    private void OnMouseDown()
    {
        if (HasFish) // Check first if there is a fish
        {
            FishManager.Instance.CatchFish(CaughtFish);
            CaughtFish = FishSpecies.None;
            HasFish = false;
            DetachLure();
            StopAllCoroutines();
        }
    }

    /// <summary>
    /// Wait for a fish to be caugth
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitingForFish()
    {
        Debug.Log("waiting for fish");
        // Wait for a random time
        yield return new WaitForSeconds(Random.Range(minTimeForFish, maxTimeForFish));

        // Create catch value
        int catchValue = Random.Range(0, totalCatchScore);
        float timeAttached = 0;

        // Catch fish
        for (int i = 0; i < fishCatchChances.Length; i++)
        {
            if (catchValue >= fishCatchChances[i].minScore && catchValue < fishCatchChances[i].maxScore)
            {
                HasFish = true;
                CaughtFish = fishCatchChances[i].species;
                timeAttached = fishCatchChances[i].timeAttached;
                Debug.Log(CaughtFish + " is hooked! Score: " + catchValue);
                break;
            }
        }

        yield return new WaitForSeconds(timeAttached);

        // TODO: give a chance to lose lure
        HasFish = false;
        CaughtFish = FishSpecies.None;
        StartCoroutine(WaitingForFish());
        Debug.Log("Got away");
    }

    /// <summary>
    /// Attaches lure to rod
    /// </summary>
    /// <param name="lure"></param>
    /// <param name="catchTotal"></param>
    /// <param name="catchScores"></param>
    public void AttachLure(LureStats lure, int catchTotal, FishCatchScore[] catchScores)
    {
        LureAttached = lure;
        IsAttached = true;
        outline.enabled = false;
        totalCatchScore = catchTotal;
        fishCatchChances = catchScores;
        StartCoroutine(WaitingForFish());
    }

    /// <summary>
    /// Removes lure from rod
    /// </summary>
    public void DetachLure()
    {
        // TODO: display fish and lure before destroying lure
        LureAttached = null;
        IsAttached = false;
        outline.enabled = false;
        StopAllCoroutines();
    }
}
