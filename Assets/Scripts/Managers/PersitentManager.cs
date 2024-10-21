using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main singleton used to transfer and store data between scenes
/// </summary>
public class PersitentManager : MonoBehaviour
{
    public static PersitentManager Instance { get; private set; }

    // Lure
    private readonly List<LureProperties> luresCreated = new();

    // Fish
    [SerializeField] private Fish[] everyFishInGame;                // Contains every fish that exists (set in inspector)
    private List<FishSpecies> fishForThisLevel = new();             // Fishes available for this level (Set before loading to a level)
    private readonly Dictionary<FishSpecies, Fish> fishDict = new();// Dictionary that is set during runtime    

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
        fishForThisLevel = new()
        {
            FishSpecies.Dipfish,
            FishSpecies.Bobber,
            FishSpecies.Pickley,
            FishSpecies.Fry
        };

        // Populate dictionary
        for (int i = 0; i < everyFishInGame.Length; i++)
        {
            fishDict.Add(everyFishInGame[i].Species, everyFishInGame[i]);
        }
    }

    public void AddLure(GameObject oriObj)
    {
        oriObj.SetActive(false);
        GameObject newObj = Instantiate(oriObj, transform);

        if (newObj.TryGetComponent<BlockRotation>(out var rot))
        {
            Destroy(rot);
        }

        luresCreated.Add(newObj.GetComponent<LureProperties>());
        oriObj.SetActive(true);
    }

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

    public LureProperties GetLure()
    {
        if (luresCreated.Count > 0)
        {
            LureProperties nextLure = luresCreated[^1];
            luresCreated.Remove(nextLure);
            return nextLure;
        }

        return null;
    }
}
