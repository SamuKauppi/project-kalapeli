using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main singleton used to transfer and store data between scenes
/// </summary>
public class PersitentManager : MonoBehaviour
{
    public static PersitentManager Instance { get; private set; }

    // Lure
    private readonly List<GameObject> luresCreated = new();

    // Fish
    [SerializeField] private List<Fish> everyFishInGame = new();    // Contains every fish that exists (set in inspector)
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
            FishSpecies.Bobber
        };

        // Populate dictionary
        for (int i = 0; i < everyFishInGame.Count; i++)
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

        if (newObj.TryGetComponent<LureProperties>(out var prop))
        {
            prop.FinishLure();
        }

        luresCreated.Add(newObj);
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
}
