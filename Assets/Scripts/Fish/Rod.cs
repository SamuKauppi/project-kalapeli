using System.Collections;
using UnityEngine;

public class Rod : MonoBehaviour
{
    public bool IsAttached { get; private set; } = false;
    public LureProperties LureAttached { get; private set; } = null;
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
        if (!IsAttached)
        {
            FishManager.Instance.OnLureClick(this);
        }
        else if (HasFish)
        {
            FishManager.Instance.CatchFish(CaughtFish);
            CaughtFish = FishSpecies.None;
            HasFish = false;
            DetachLure();
        }
    }

    private IEnumerator WaitingForFish()
    {
        yield return new WaitForSeconds(Random.Range(minTimeForFish, maxTimeForFish));
        int catchValue = Random.Range(0, totalScore);
        for (int i = 0; i < fishCatchChances.Length; i++)
        {
            if (catchValue >= fishCatchChances[i].minScore && catchValue < fishCatchChances[i].maxScore)
            {
                HasFish = true;
                CaughtFish = fishCatchChances[i].species;
                break;
            }
        }

        DetachLure();
    }

    public void AttachLure(LureProperties lure, int catchTotal, FishCatchScore[] catchScores)
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
