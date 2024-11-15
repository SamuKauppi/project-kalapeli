using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Main singleton used to transfer and store data between scenes
/// </summary>
public class PersitentManager : MonoBehaviour
{
    public static PersitentManager Instance { get; private set; }
    public Fish[] EveryFish { get {  return everyFishInGame; } }

    // Lure
    private readonly Queue<GameObject> luresCreated = new();
    private readonly HashSet<GameObject> luresUsed = new();

    // Fish
    [SerializeField] private Fish[] everyFishInGame;                    // Contains every fish that exists (set in inspector)
    [SerializeField] private List<FishSpecies> fishForThisLevel = new();// Fishes available for this level (Set before loading to a level)
    private readonly Dictionary<FishSpecies, Fish> fishDict = new();    // Dictionary that is set during runtime
    private readonly HashSet<FishSpecies> fishesCaught = new();

    // Score
    [SerializeField] private TMP_Text scoreText;
    private int score;
    private const string SCORE = "Score: ";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Temporary way to populate fishForThisLevel
        // TODO: create a function to do this once menu exists

        // Populate dictionary
        for (int i = 0; i < everyFishInGame.Length; i++)
        {
            fishDict.Add(everyFishInGame[i].Species, everyFishInGame[i]);
        }

        scoreText.text = SCORE + score;
    }

    /// <summary>
    /// Returns the fishes available for this level
    /// </summary>
    /// <returns></returns>
    public Fish[] GetFishesForThisLevel()
    {
        Fish[] fishes = new Fish[fishForThisLevel.Count];

        for (int i = 0; i < fishes.Length; i++)
        {
            if (fishDict.ContainsKey(fishForThisLevel[i]))
            {
                fishes[i] = fishDict[fishForThisLevel[i]];
            }
        }

        return fishes;
    }

    /// <summary>
    /// Adds a lure to list
    /// </summary>
    /// <param name="oriObj"></param>
    public void AddNewLure(GameObject oriObj)
    {
        oriObj.SetActive(false);
        GameObject newObj = Instantiate(oriObj, transform);

        if (newObj.TryGetComponent<BlockRotation>(out var rot))
        {
            Destroy(rot);
        }

        if (newObj.TryGetComponent<LureFunctions>(out var func))
        {
            Destroy(func);
        }

        if(newObj.TryGetComponent<Collider>(out var coll))
        {
            Destroy(coll);
        }

        luresCreated.Enqueue(newObj);
        oriObj.SetActive(true);
    }

    /// <summary>
    /// Adds lure back to queue if it was not used properly
    /// </summary>
    /// <param name="lure"></param>
    public void ReaddLure(GameObject lure)
    {
        luresCreated.Enqueue(lure);
        luresUsed.Remove(lure);
        lure.SetActive(false);
    }

    /// <summary>
    /// Returns next available lure
    /// </summary>
    /// <returns></returns>
    public GameObject GetLure()
    {
        if (luresCreated.Count <= 0)
        {
            return null;
        }

        GameObject lure = luresCreated.Dequeue();
        lure.SetActive(true);
        luresUsed.Add(lure);
        return lure;
    }

    public void GainScoreFormFish(FishSpecies fish)
    {
        int value = fishDict[fish].ScoreGained;
        score += value;
        scoreText.text = SCORE + score;
        fishesCaught.Add(fish);
    }
}
