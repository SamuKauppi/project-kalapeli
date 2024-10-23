using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages fishing portion of the game
/// </summary>
public class FishManager : MonoBehaviour
{
    public static FishManager Instance { get; private set; }

    [SerializeField] private Rod rodPrefab;                 // Prefab
    [SerializeField] private Transform[] rodAttachPoints;   // Points where rods are attached
    [SerializeField] private Transform fishDisplay;

    private Fish[] availableFish;   // Fishes for this level
    private Rod[] rods;             // Rods made during runtime

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
            rods[i] = Instantiate(rodPrefab, rodAttachPoints[i].position, rodAttachPoints[i].rotation, transform);
        }
    }

    /// <summary>
    /// When that does not have a lure is clicked
    /// Calculate catch chances for each fish and attach lure to rod
    /// </summary>
    /// <param name="targetRod"></param>
    public void OnRodClick(Rod targetRod)
    {
        // Get next lure
        GameObject lure = PersitentManager.Instance.GetLure();
        if (lure == null) { return; }

        // Ensure that LureStats is found
        if (lure.TryGetComponent<LureStats>(out var nextLure))
        {
            // Create a list of fishes that can be caught
            List<FishCatchScore> fishCatchScores = new();
            int totalScore = 0;

            for (int i = 0; i < availableFish.Length; i++)
            {
                int score = availableFish[i].GetCatchChance(nextLure);

                // If Fish can be caught, add it to list and increment totalScore
                if (score > 0)
                {
                    FishCatchScore fcs = new()
                    {
                        species = availableFish[i].Species,
                        minScore = totalScore
                    };

                    totalScore += score;
                    fcs.maxScore = totalScore;
                    fishCatchScores.Add(fcs);
                }
                Debug.Log("Fish: " + availableFish[i].Species + ", Score: " + score);
            }
            // Attach lure and it's catch chances to rod
            targetRod.AttachLure(nextLure, totalScore, fishCatchScores.ToArray());
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
                Instantiate(fish.gameObject, fishDisplay.transform.position, fishDisplay.transform.rotation, fishDisplay);
                break;
            }
        }
    }

    /// <summary>
    /// End fishing mode
    /// </summary>
    public void EndFishing()
    {
        GameManager.Instance.SwapModes();
    }
}
