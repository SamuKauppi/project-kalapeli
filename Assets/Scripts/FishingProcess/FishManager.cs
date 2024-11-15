using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages fishing portion of the game
/// </summary>
public class FishManager : MonoBehaviour
{
    public static FishManager Instance { get; private set; }
    public bool CanFish { get; set; }
    public bool IsAttachingLure { get; private set; }

    [SerializeField] private PickUpLure lureBox;

    // Rod
    [SerializeField] private Rod rodPrefab;                 // Prefab
    [SerializeField] private Transform[] rodAttachPoints;   // Points where rods are attached
    [SerializeField] private LayerMask rodLayer;            // Layer Mask for rods
    [SerializeField] private float raycastDistance;         // How far does the ray check
    [SerializeField] private float lurePositionFromCam;     // Position away from camera

    // Fish display
    [SerializeField] private Transform displayParent;       // Parent for displaying fish   
    [SerializeField] private GameObject displayUI;          // For displaying UI when fish is caught
    [SerializeField] private TMP_Text scoreText;            // Ui to display score
    [SerializeField] private GameObject backButton;         // Back button for ui
    private GameObject displayFish;                         // Fish being displayed
    private const string SCORE = "Score: ";

    private Fish[] availableFish;   // Fishes for this level
    private Rod[] rods;             // Rods made during runtime

    private GameObject attachedLure;
    private Camera cam;
    private Vector3 mousePos;

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
        // Get fishes for this level
        availableFish = PersitentManager.Instance.GetFishesForThisLevel();

        // Create rods
        rods = new Rod[rodAttachPoints.Length];
        for (int i = 0; i < rodAttachPoints.Length; i++)
        {
            rods[i] = Instantiate(rodPrefab, rodAttachPoints[i].position, rodAttachPoints[i].rotation, rodAttachPoints[i]);
        }

        cam = GameManager.Instance.MainCamera;
        displayUI.SetActive(false);
        backButton.SetActive(true);
    }

    private void Update()
    {
        if (!IsAttachingLure || !CanFish) { return; }
        mousePos = Input.mousePosition;
        mousePos.z = lurePositionFromCam;

        if (Input.GetMouseButton(0))
        {
            attachedLure.transform.position = cam.ScreenToWorldPoint(mousePos);
        }

        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = new(cam.transform.position, (cam.ScreenToWorldPoint(mousePos) - cam.transform.position).normalized);

            // Perform the raycast to check if lure can be attached to rod
            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, rodLayer))
            {
                if (hit.collider.TryGetComponent(out Rod rod) && !rod.IsAttached)
                {
                    ActivateRod(rod, attachedLure);
                    return;
                }
            }

            DropLure();
        }
    }

    private void DropLure()
    {
        if (attachedLure != null)
            PersitentManager.Instance.ReaddLure(attachedLure);
        IsAttachingLure = false;
        attachedLure = null;
    }

    public void PickUpLure()
    {
        GameObject lureObj = PersitentManager.Instance.GetLure();
        if (lureObj == null) { return; }
        attachedLure = lureObj;
        IsAttachingLure = true;
    }

    /// <summary>
    /// Activates a rod with given lure
    /// </summary>
    /// <param name="targetRod"></param>
    /// <param name="lure"></param>
    public void ActivateRod(Rod targetRod, GameObject lure)
    {
        // Ensure that LureStats is found
        if (lure.TryGetComponent<LureStats>(out var nextLure))
        {
            // Create a list of fishes that can be caught
            List<FishCatchScore> fishCatchScores = new();
            int totalScore = 0;

            for (int i = 0; i < availableFish.Length; i++)
            {
                int score = availableFish[i].GetCatchChance(nextLure);

                // If Fish can be caught, add it to list and increment totalCatchScore
                if (score > 0)
                {
                    FishCatchScore fcs = new()
                    {
                        species = availableFish[i].Species,
                        minScore = totalScore,
                        timeAttached = availableFish[i].TimeAttached,
                    };

                    totalScore += score;
                    fcs.maxScore = totalScore;
                    fishCatchScores.Add(fcs);
                }
                //Debug.Log("Fish: " + availableFish[i].Species + ", Score: " + score);
            }

            // Attach lure and it's catch chances to rod
            targetRod.AttachLure(nextLure, totalScore, fishCatchScores.ToArray());

            // Hide lure and enable taking a new lure
            attachedLure.SetActive(false);
            attachedLure = null;
            IsAttachingLure = false;
            lureBox.OpenLureBox(PersitentManager.Instance.LureCount());
        }
        else
        {
            DropLure();
        }
    }

    /// <summary>
    /// Catch and display given fish species
    /// </summary>
    /// <param name="caughtFish"></param>
    public void CatchFish(FishSpecies caughtFish)
    {
        // TODO: display fish properly after catching
        foreach (Fish fish in availableFish)
        {
            if (fish.Species.Equals(caughtFish))
            {
                displayFish = Instantiate(fish.gameObject, displayParent.transform.position, displayParent.transform.rotation, displayParent);
                displayUI.SetActive(true);
                backButton.SetActive(false);
                PersitentManager.Instance.GainScoreFormFish(fish.Species);
                scoreText.text = SCORE + fish.ScoreGained;
                break;
            }
        }
    }


    public void AcceptFish()
    {
        displayUI.SetActive(false);
        backButton.SetActive(true);
        if (displayFish)
            Destroy(displayFish);

    }


    /// <summary>
    /// End fishing mode
    /// </summary>
    public void EndFishing()
    {
        GameManager.Instance.SwapModes();
    }

    public void ActivateFishing()
    {
        lureBox.OpenLureBox(PersitentManager.Instance.LureCount());
    }
}
