using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Main singleton used to transfer and store data between scenes
/// </summary>
public class PersitentManager : MonoBehaviour
{
    public static PersitentManager Instance { get; private set; }
    public Fish[] EveryFish { get { return everyFishInGame; } }

    // Lure
    private readonly List<LureStats> luresCreated = new();

    // Fish
    [SerializeField] private Fish[] everyFishInGame;                    // Contains every fish that exists (set in inspector)
    [SerializeField] private List<FishSpecies> fishForThisLevel = new();// Fishes available for this level (Set before loading to a level)
    private readonly Dictionary<FishSpecies, Fish> fishDict = new();    // Dictionary that is set during runtime
    private readonly Dictionary<FishSpecies, int> fishesCaught = new();

    // Score
    [SerializeField] private TMP_Text scoreText;
    private int score;
    private int lures_lost;
    private int lures_made;
    private int attachedUsed;
    private int cuts;
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
        // Populate dictionary
        for (int i = 0; i < everyFishInGame.Length; i++)
        {
            everyFishInGame[i].InitializeFish();
            fishDict.Add(everyFishInGame[i].Species, everyFishInGame[i]);
            int count = PlayerPrefManager.Instance.GetFishValue(everyFishInGame[i].Species, 0);
            if (count > 0)
            {
                fishesCaught[everyFishInGame[i].Species] = count;
            }
        }

        scoreText.text = SCORE + score;

        score = PlayerPrefManager.Instance.GetPrefValue(SaveValue.score, 0);
        lures_lost = PlayerPrefManager.Instance.GetPrefValue(SaveValue.lures_lost, 0);
        lures_made = PlayerPrefManager.Instance.GetPrefValue(SaveValue.lures_made, 0);
        attachedUsed = PlayerPrefManager.Instance.GetPrefValue(SaveValue.decorations, 0);
        cuts = PlayerPrefManager.Instance.GetPrefValue(SaveValue.cuts, 0);
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
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
        SetLayerRecursively(newObj, 31);

        if (newObj.TryGetComponent<BlockRotation>(out var rot))
        {
            Destroy(rot);
        }

        if (newObj.TryGetComponent<LureFunctions>(out var func))
        {
            Destroy(func);
        }

        if (newObj.TryGetComponent<Collider>(out var coll))
        {
            Destroy(coll);
        }

        luresCreated.Add(newObj.GetComponent<LureStats>());
        oriObj.SetActive(true);
    }

    /// <summary>
    /// Adds lure back to queue if it was not used properly
    /// </summary>
    /// <param name="lure"></param>
    public void AddLure(LureStats lure)
    {
        luresCreated.Add(lure);
    }

    public void TakeLure(LureStats lure)
    {
        luresCreated.Remove(lure);
    }

    public LureStats[] GetLureArray()
    {
        return luresCreated.ToArray();
    }

    public int LureCount()
    {
        return luresCreated.Count;
    }


    public void GainScoreFormFish(FishSpecies fish, Vector3 particlePos)
    {
        int value = fishesCaught.ContainsKey(fish) ? fishDict[fish].ScoreGained : fishDict[fish].ScoreGained / 2;
        score += value;
        scoreText.text = SCORE + score;

        ParticleEffectManager.Instance.PlayParticleEffect(ParticleType.DisplayFish, particlePos);

        if (!fishesCaught.ContainsKey(fish))
        {
            ParticleEffectManager.Instance.PlayParticleEffect(ParticleType.DisplayNewFish, particlePos);
            Tutorial.Instance.CatchNewFish();
        }

        fishesCaught[fish]++;
        ScorePage.Instance.UpdateNonFishValue(SaveValue.score, score);
        ScorePage.Instance.UpdateFishValue(fish, 1);
    }

    public bool IsFishCaught(FishSpecies fish)
    {
        return fishesCaught.ContainsKey(fish);
    }
}