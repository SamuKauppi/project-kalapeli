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

    // Line Renderer
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform lineStartPoint;
    private Vector3 noFishLinePoint;
    private Vector3 fishLinePoint;
    [SerializeField] private float lineStartWidth;
    [SerializeField] private float lineEndWidth;

    // Min & Max time to wait for fish to be caught
    [SerializeField] private float minTimeForFish;
    [SerializeField] private float maxTimeForFish;

    // Got from FishManager
    private FishCatchScore[] fishCatchChances;

    // Total catchscore
    private int totalCatchScore;

    private void Start()
    {
        lineRenderer.startWidth = lineStartWidth;
        lineRenderer.endWidth = lineEndWidth;
        lineRenderer.enabled = false;
        lineRenderer.material.renderQueue = 3001;
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
        if (HasFish && FishManager.Instance.CanFish && !FishManager.Instance.IsHoldingLure) // Check first if there is a fish
        {
            FishManager.Instance.CatchFish(CaughtFish, LureAttached);
            CaughtFish = FishSpecies.None;
            HasFish = false;
            DetachLure();
            StopAllCoroutines();
            lineRenderer.enabled = false;
        }
    }

    private void Update()
    {
        if (!IsAttached)
        {
            return;
        }

        lineRenderer.SetPosition(0, lineStartPoint.position);

        if (HasFish && lineRenderer.GetPosition(0) != fishLinePoint)
        {
            lineRenderer.SetPosition(1, fishLinePoint);
        }
        else if(lineRenderer.GetPosition(0) != noFishLinePoint)
        {
            lineRenderer.SetPosition(1, noFishLinePoint);
        }
    }

    /// <summary>
    /// Wait for a fish to be caugth
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitingForFish()
    {
        // Wait for a random time
        if (fishCatchChances[0].species != FishSpecies.Boot)
        {
            PrintCatchChances(fishCatchChances, totalCatchScore);
            yield return new WaitForSeconds(Random.Range(minTimeForFish, maxTimeForFish));
        }
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
                anim.SetBool("Fish", true);
                sound.Play();
                outline.enabled = true;
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
        outline.enabled = false;
    }

    // Function to calculate and print catch chances
    private void PrintCatchChances(FishCatchScore[] fishCatchScores, int totalScore)
    {
        foreach (var fish in fishCatchScores)
        {
            int fishScore = fish.maxScore - fish.minScore;
            float catchPercentage = (fishScore / (float)totalScore) * 100f;
            Debug.Log($"Fish: {fish.species}, Catch Chance: {catchPercentage:F2}%");
        }
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
        lineRenderer.enabled = true;
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

    public void SetLineEndPoint(Vector3 noFishEndPoint, Vector3 fishEndPoint)
    {
        noFishLinePoint = noFishEndPoint;
        fishLinePoint = fishEndPoint;
        lineRenderer.SetPosition(1, noFishEndPoint);
    }
}
