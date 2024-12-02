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
    [SerializeField] private Outline outline;
    [SerializeField] private Animator anim;

    // Line
    [SerializeField] private GameObject lineObject;
    [SerializeField] private Transform lineBone1;
    [SerializeField] private Transform lineBone2;
    [SerializeField] private Transform lineStartPoint;
    [SerializeField] private Transform[] rodBones;
    [SerializeField] private float minRotationPercentage;
    [SerializeField] private float maxRotationPercentage;
    [SerializeField] private float rodAnimationSpeed;
    private float rotationPercentage;
    private float rotationDir = 1f;
    private Transform noFishLinePoint;
    private Transform fishLinePoint;
    private Vector3 intersectingPoint;

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
        lineObject.SetActive(false);
        rotationPercentage = minRotationPercentage;
    }

    private void OnMouseEnter()
    {
        if (FishManager.Instance.CanFish && (IsAttached || HasFish || FishManager.Instance.IsHoldingLure))
        {
            outline.enabled = true;
        }
    }

    private void OnMouseExit()
    {
        if (!HasFish)
            outline.enabled = false;
    }

    private void OnMouseDown()
    {
        if (FishManager.Instance.CanFish && !FishManager.Instance.IsHoldingLure)
        {
            if (HasFish) // Handle the case when there is a fish
            {
                FishManager.Instance.CatchFish(CaughtFish);
                PersitentManager.Instance.AddLure(LureAttached);
                CaughtFish = FishSpecies.None;
                HasFish = false;
                DetachLure();
                StopAllCoroutines();
            }
            else if (IsAttached) // Handle the case when there is no fish but the lure is attached
            {
                PersitentManager.Instance.AddLure(LureAttached);
                DetachLure();
                StopAllCoroutines();
            }
        }
    }

    private void Update()
    {
        rotationPercentage += rodAnimationSpeed * rotationDir * Time.deltaTime;

        if (rotationPercentage > maxRotationPercentage || rotationPercentage < minRotationPercentage)
        {
            rotationDir *= -1f;
            rotationPercentage = Mathf.Clamp(rotationPercentage, minRotationPercentage, maxRotationPercentage);
        }


        if (!IsAttached)
        {
            return;
        }

        lineBone1.position = lineStartPoint.position;

        if (HasFish)
        {
            lineBone2.position = fishLinePoint.position;
        }
        else
        {
            lineBone2.position = noFishLinePoint.position;

            foreach (Transform bone in rodBones)
            {
                // Calculate the direction to the target point
                Vector3 directionToPoint = (bone.position - noFishLinePoint.position).normalized;

                // Correctly get the axis of rotation using the bone's local up direction (-y is up)
                Vector3 axis = Vector3.Cross(directionToPoint, -bone.forward);

                // Calculate the angle between the bone's forward direction (-up) and the target direction
                float angle = Vector3.Angle(-bone.up, directionToPoint);

                // Apply rotation only around the calculated axis using a quaternion
                Quaternion rotation = Quaternion.AngleAxis(angle * rotationPercentage, axis);

                // Combine the current local rotation with the new rotation
                bone.localRotation = rotation * Quaternion.identity;
            }

        }
    }

    /// <summary>
    /// Wait for a fish to be caught
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitingForFish()
    {

        // Wait for a random time
        if (fishCatchChances.Length > 0 && fishCatchChances[0].species != FishSpecies.Boot)
        {
#if UNITY_EDITOR
            PrintCatchChances(fishCatchChances, totalCatchScore);
#endif
            float waitTime = maxTimeForFish;
            yield return new WaitForSeconds(Random.Range(minTimeForFish, waitTime));
        }
        else
            yield return new WaitForSeconds(3f);

        // Create catch score
        int catchValue = Random.Range(0, totalCatchScore);
        float timeAttached = 0;

        // Catch fish
        for (int i = 0; i < fishCatchChances.Length; i++)
        {
            if (catchValue < fishCatchChances[i].minScore
                || catchValue >= fishCatchChances[i].maxScore)
            {
                continue;
            }

            if (fishCatchChances[i].species == FishSpecies.None)
            {
                break;
            }

            HasFish = true;
            CaughtFish = fishCatchChances[i].species;
            timeAttached = fishCatchChances[i].timeAttached;
            anim.enabled = true;
            anim.SetBool("Fish", true);
            outline.enabled = true;
            break;
        }

        yield return new WaitForSeconds(timeAttached);
        SoundManager.Instance.PlaySound(SoundClipTrigger.OnFishAlert);
        ScorePage.Instance.UpdateNonFishValue(SaveValue.fishes_missed, 1);

        HasFish = false;
        CaughtFish = FishSpecies.None;
        anim.SetBool("Fish", false);
        outline.enabled = false;

        if (Random.Range(0, 6) == 0)
            StartCoroutine(WaitingForFish());
        else
            DestroyLure();

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

    private void ResetFishingRodRotation()
    {
        foreach (Transform rod in rodBones)
        {
            rod.localRotation = Quaternion.identity;
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
        totalCatchScore = catchTotal;
        fishCatchChances = catchScores;
        StartCoroutine(WaitingForFish());
        anim.enabled = false;
        lineObject.SetActive(true);
        SoundManager.Instance.PlaySound(SoundClipTrigger.OnCastThrow);
        CursorManager.Instance.SwapCursor(CursorType.Normal);

        intersectingPoint = WaterSurfacePoint.Instance.GetIntersectingPoint(lineStartPoint.position, noFishLinePoint.position);
        ParticleEffectManager.Instance.PlayParticleEffect(ParticleType.Splash, intersectingPoint);

        ResetFishingRodRotation();
    }

    /// <summary>
    /// Removes lure from rod
    /// </summary>
    public void DetachLure()
    {
        lineObject.SetActive(false);
        LureAttached = null;
        IsAttached = false;
        outline.enabled = false;
        StopAllCoroutines();
        anim.enabled = true;
        anim.SetBool("Water", false);
        anim.SetBool("Fish", false);
        FishingLureBox.Instance.SetLureBoxActive(PersitentManager.Instance.LureCount());
    }

    public void DestroyLure()
    {
        Destroy(LureAttached.gameObject);
        DetachLure();
        ScorePage.Instance.UpdateNonFishValue(SaveValue.lures_lost, 1);
    }

    public void SetLineEndPoint(Transform noFishEndPoint, Transform fishEndPoint)
    {
        noFishLinePoint = noFishEndPoint;
        fishLinePoint = fishEndPoint;
    }
}
