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

    // References
    [SerializeField] private AudioSource sound;
    [SerializeField] private Outline outline;
    [SerializeField] private Animator anim;

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
        if ((!IsAttached || HasFish) && FishManager.Instance.CanFish)
        {
            outline.enabled = true;
        }
    }

    private void OnMouseExit()
    {
        if (!IsAttached || HasFish)
            outline.enabled = false;
    }

    private void OnMouseDown()
    {
        if (HasFish && FishManager.Instance.CanFish) // Check first if there is a fish
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
        if (fishCatchChances.Length > 1)
            yield return new WaitForSeconds(Random.Range(minTimeForFish, maxTimeForFish));
        else
            yield return new WaitForSeconds(3f);

        // Create catch isCatalogOpen
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
                anim.SetBool("Fish", true);
                sound.Play();
                break;
            }
        }

        yield return new WaitForSeconds(timeAttached);

        // TODO: give a chance to lose lure
        HasFish = false;
        CaughtFish = FishSpecies.None;
        StartCoroutine(WaitingForFish());
        Debug.Log("Got away");
        anim.SetBool("Fish", false);
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
        anim.SetBool("Water", true);
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
        anim.SetBool("Water", false);
        anim.SetBool("Fish", false);
    }
}
