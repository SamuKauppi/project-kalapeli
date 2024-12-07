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
    private FishCatchScore[] fishCatchChances;

    // Line
    [SerializeField] private GameObject lineObject;         // Fishingline
    [SerializeField] private Transform lineBone1;           // Fishingline bone which stays at the tip of the rod
    [SerializeField] private Transform lineBone2;           // Fishingline bone which moves at the bottom of the ocean
    [SerializeField] private Transform lineStartPoint;      // Point on the rod where the linebone1 stays
    [SerializeField] private Transform[] rodBones;          // Fishingrod bones that are bent while fishing
    [SerializeField] private float minRotationPercentage;   // Min rotation per bone for rodBones
    [SerializeField] private float maxRotationPercentage;   // Max rotation per bone for rodBones
    [SerializeField] private float rodAnimationSpeed;       // How fast the rotation changes between min and max
    private float rotationPercentage;           // Current rotation percentage
    private float rotationDir = 1f;             // Which direction is rotation changing
    private Transform noFishLinePoint;          // Where the lineBone2 ends when there is no fish
    private Transform fishLinePoint;            // Where the lineBone2 ends when there is a fish

    // Min & Max time to wait for fish to be caught
    [SerializeField] private float minTimeForFish;
    [SerializeField] private float maxTimeForFish;

    // Total catchscore of the current lure
    private int totalCatchScore;

    private void Start()
    {
        outline.enabled = false;
        lineObject.SetActive(false);
        rotationPercentage = minRotationPercentage;
    }

    /// <summary>
    /// When player mouse enters rod
    /// display outline when there is a fish or player is holding lure when rod does not have fish
    /// </summary>
    private void OnMouseEnter()
    {
        if (FishManager.Instance.CanFish && (HasFish || (FishManager.Instance.IsHoldingLure && !IsAttached)))
        {
            outline.enabled = true;
        }
    }

    /// <summary>
    /// Disable outline when there is no fish
    /// </summary>
    private void OnMouseExit()
    {
        if (!HasFish)
            outline.enabled = false;
    }

    /// <summary>
    /// Handle here picking up empty lure and catching fish
    /// </summary>
    private void OnMouseDown()
    {
        if (FishManager.Instance.CanFish && !FishManager.Instance.IsHoldingLure)
        {
            if (HasFish) // Handle the case when there is a fish
            {
                // Catch a fish and add lure back to box
                FishManager.Instance.CatchFish(CaughtFish);
                PersitentManager.Instance.AddLure(LureAttached);

                // Reset variable
                CaughtFish = FishSpecies.None;
                HasFish = false;

                // Detach lure and stop all coroutines
                DetachLure();
                StopAllCoroutines();
            }
            else if (IsAttached) // Handle the case when there is no fish but the lure is attached
            {
                // Add lure back to box
                PersitentManager.Instance.AddLure(LureAttached);

                // Detach lure and stop all coroutines
                DetachLure();
                StopAllCoroutines();
            }
        }
    }

    private void Update()
    {
        // Always calculate the current rotation percentage to keep rods in sync
        CalculateRotationPercentage();

        if (!IsAttached)
        {
            return;
        }

        UpdateLineAndRod();
    }

    /// <summary>
    /// Updates the position of lineBones and the rotation of rodBones
    /// </summary>
    private void UpdateLineAndRod()
    {
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
                Vector3 directionToPoint = (bone.position - noFishLinePoint.position).normalized;

                Vector3 axis = Vector3.Cross(directionToPoint, -bone.forward);

                float angle = Vector3.Angle(-bone.up, directionToPoint) * rotationPercentage;

                Quaternion rotation = Quaternion.AngleAxis(angle, axis);

                bone.localRotation = rotation * Quaternion.identity;

                bone.Rotate(Vector3.forward, angle, Space.Self);
            }

        }
    }

    /// <summary>
    /// Calculates rotation percetage
    /// </summary>
    private void CalculateRotationPercentage()
    {
        rotationPercentage += rodAnimationSpeed * rotationDir * Time.deltaTime;

        if (rotationPercentage > maxRotationPercentage || rotationPercentage < minRotationPercentage)
        {
            rotationDir *= -1f;
            rotationPercentage = Mathf.Clamp(rotationPercentage, minRotationPercentage, maxRotationPercentage);
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
            float waitTime = maxTimeForFish;
            yield return new WaitForSeconds(Random.Range(minTimeForFish, waitTime));
        }
        // or flat 3 seconds if the fish is a boot
        else
            yield return new WaitForSeconds(3f);

        // Hook a fish and wait some time based on fish
        float timeAttached = HookFish();
        yield return new WaitForSeconds(timeAttached);

        // Miss a fish that was hooked
        MissFish();
    }

    /// <summary>
    /// Handles situation when fish is missed by the player
    /// </summary>
    private void MissFish()
    {
        // Update score
        ScorePage.Instance.UpdateNonFishValue(SaveValue.fishes_missed, 1);

        // Set variables and animation
        HasFish = false;
        CaughtFish = FishSpecies.None;
        anim.SetBool("Fish", false);
        anim.enabled = false;
        outline.enabled = false;

        // Give a small chance to destroy the lure
        if (Random.Range(0, 6) == 0)
            StartCoroutine(WaitingForFish());
        else
            DestroyLure();
    }

    /// <summary>
    /// Hooks a random fish based on total totalCatchScore
    /// </summary>
    /// <returns>How long this fish will remain hooked</returns>
    private float HookFish()
    {
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
            Tutorial.Instance.FishAlert();
            SoundManager.Instance.PlaySound(SoundClipTrigger.OnFishAlert);
            break;
        }

        return timeAttached;
    }

    /// <summary>
    /// Resets the rotation of rodBones to 0
    /// </summary>
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
        // Set variables
        LureAttached = lure;
        IsAttached = true;
        totalCatchScore = catchTotal;
        fishCatchChances = catchScores;
        anim.enabled = false;
        lineObject.SetActive(true);

        // Start fishing coroutine
        StartCoroutine(WaitingForFish());

        // Play sound and swap cursor to normal
        SoundManager.Instance.PlaySound(SoundClipTrigger.OnCastThrow);
        CursorManager.Instance.SwapCursor(CursorType.Normal);

        // Play particle effect at intersecting point
        Vector3 intersectingPoint = WaterSurfacePoint.Instance.GetIntersectingPoint(lineStartPoint.position, noFishLinePoint.position);
        ParticleEffectManager.Instance.PlayParticleEffect(ParticleType.Splash, intersectingPoint);

        // Reset the rotation of the rodBones before animating it
        ResetFishingRodRotation();
    }

    /// <summary>
    /// Removes lure from rod
    /// </summary>
    public void DetachLure()
    {
        // Set variables
        lineObject.SetActive(false);
        LureAttached = null;
        IsAttached = false;
        outline.enabled = false;
        anim.enabled = true;
        anim.SetBool("Water", false);
        anim.SetBool("Fish", false);

        // Stop coroutines
        StopAllCoroutines();

        // Update the lurebox visuals
        FishingLureBox.Instance.SetLureBoxActive(PersitentManager.Instance.LureCount());
    }

    /// <summary>
    /// Destroys the lure
    /// </summary>
    public void DestroyLure()
    {
        Destroy(LureAttached.gameObject);
        DetachLure();
        ScorePage.Instance.UpdateNonFishValue(SaveValue.lures_lost, 1);
    }

    /// <summary>
    /// Sets references to variables
    /// </summary>
    /// <param name="noFishEndPoint"></param>
    /// <param name="fishEndPoint"></param>
    public void SetLineEndPoint(Transform noFishEndPoint, Transform fishEndPoint)
    {
        noFishLinePoint = noFishEndPoint;
        fishLinePoint = fishEndPoint;
    }
}
