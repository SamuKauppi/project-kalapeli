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
        availableFish = PersitentManager.Instance.GetFishesForThisLevel();

        rods = new Rod[rodAttachPoints.Length];
        for (int i = 0; i < rodAttachPoints.Length; i++)
        {
            rods[i] = Instantiate(rodPrefab, rodAttachPoints[i].position, rodAttachPoints[i].rotation, transform);
        }
    }

    public void OnLureClick(Rod targetRod)
    {
        LureProperties nextLure = PersitentManager.Instance.GetLure();
        if (nextLure != null)
        {
            List<FishCatchScore> fishCatchScores = new();
            int totalScore = 0;

            for (int i = 0; i < availableFish.Length; i++)
            {
                int score = availableFish[i].GetCatchChance(nextLure);
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
            targetRod.AttachLure(nextLure, totalScore, fishCatchScores.ToArray());
        }
    }

    public void CatchFish(FishSpecies fish)
    {
        Debug.Log("You caught: " + fish);
    }


    public void EndFishing()
    {
        GameManager.Instance.SwapModes();
    }
}
