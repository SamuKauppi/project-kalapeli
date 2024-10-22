using System.Collections;
using UnityEngine;

public class Rod : MonoBehaviour
{
    public bool IsAttached { get; private set; } = false;
    public LureStats LureAttached { get; private set; } = null;
    public FishSpecies CaughtFish { get; private set; } = FishSpecies.None;
    public bool HasFish { get; private set; } = false;

    [SerializeField] private Outline outline;
    [SerializeField] private float minTimeForFish;
    [SerializeField] private float maxTimeForFish;

    // Got from FishManager
    private FishCatchScore[] fishCatchChances;
    private int totalScore;

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
        if (HasFish) // Check if there is a fish first
        {
            FishManager.Instance.CatchFish(CaughtFish);
            CaughtFish = FishSpecies.None;
            HasFish = false;
            DetachLure();
        }
        else if (!IsAttached) // Check if the lure is not attached next
        {
            FishManager.Instance.OnLureClick(this);
        }

    }

    private IEnumerator WaitingForFish()
    {
        yield return new WaitForSeconds(Random.Range(minTimeForFish, maxTimeForFish));

        int catchValue = Random.Range(0, totalScore);
        Debug.Log("finding fish with: " + catchValue);
        for (int i = 0; i < fishCatchChances.Length; i++)
        {
            if (catchValue >= fishCatchChances[i].minScore && catchValue < fishCatchChances[i].maxScore)
            {
                HasFish = true;
                CaughtFish = fishCatchChances[i].species;
                Debug.Log(CaughtFish + " is hooked!");
                break;
            }
        }
    }

    public void AttachLure(LureStats lure, int catchTotal, FishCatchScore[] catchScores)
    {
        LureAttached = lure;
        IsAttached = true;
        outline.enabled = false;
        totalScore = catchTotal;
        fishCatchChances = catchScores;
        StartCoroutine(WaitingForFish());
    }

    public void DetachLure()
    {
        Destroy(LureAttached.gameObject);
        LureAttached = null;
        IsAttached = false;
        outline.enabled = false;
        StopAllCoroutines();
    }
}
