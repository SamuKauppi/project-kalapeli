using TMPro;
using UnityEngine;

public class ScorePage : MonoBehaviour
{
    public static ScorePage Instance { get; private set; }

    // Fishes
    [SerializeField] private TMP_Text[] fishNames;
    [SerializeField] private TMP_Text[] fishScores;

    // Score
    [SerializeField] private TMP_Text scoreTxt;
    [SerializeField] private TMP_Text lures_made;
    [SerializeField] private TMP_Text lures_lost;
    [SerializeField] private TMP_Text decorations;
    [SerializeField] private TMP_Text cuts;
    [SerializeField] private TMP_Text fishes_missed;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        Fish[] everyFish = PersitentManager.Instance.EveryFish;
        for (int i = 0; i < everyFish.Length; i++)
        {
            int count = PlayerPrefManager.Instance.GetFishValue(everyFish[i].Species, 0);

            if (count > 0)
            {
                fishNames[i].text = $"{GetFishPluralName(everyFish[i].Species)} caught:";
                fishNames[i].gameObject.SetActive(true);
                fishScores[i].text = count.ToString();
                fishScores[i].gameObject.SetActive(true);
            }
            else
            {
                fishNames[i].text = "???";
                fishScores[i].gameObject.SetActive(false);
            }
        }

        scoreTxt.text = PlayerPrefManager.Instance.GetPrefValue(SaveValue.score, 0).ToString();
        lures_made.text = PlayerPrefManager.Instance.GetPrefValue(SaveValue.lures_made, 0).ToString();
        lures_lost.text = PlayerPrefManager.Instance.GetPrefValue(SaveValue.lures_lost, 0).ToString();
        decorations.text = PlayerPrefManager.Instance.GetPrefValue(SaveValue.decorations, 0).ToString();
        cuts.text = PlayerPrefManager.Instance.GetPrefValue(SaveValue.cuts, 0).ToString();
        fishes_missed.text = PlayerPrefManager.Instance.GetPrefValue(SaveValue.fishes_missed, 0).ToString();
    }

    private string GetFishPluralName(FishSpecies fish)
    {
        return fish switch
        {
            FishSpecies.Dipfish => "Dipfishes",
            FishSpecies.Bobber => "Bobbers",
            FishSpecies.Fry => "Fries",
            FishSpecies.Pickley => "Pickles",
            FishSpecies.Boot => "Boots",
            FishSpecies.Muddle => "Muddles",
            FishSpecies.Peeper => "Peepers",
            FishSpecies.Grouchy => "Grouchies",
            _ => "",
        };
    }

    private int GetFishIndex(FishSpecies fish)
    {
        return fish switch
        {
            FishSpecies.Dipfish => 0,
            FishSpecies.Bobber => 1,
            FishSpecies.Fry => 2,
            FishSpecies.Pickley => 3,
            FishSpecies.Boot => 4,
            FishSpecies.Muddle => 5,
            FishSpecies.Peeper => 6,
            FishSpecies.Grouchy => 7,
            _ => -1,
        };
    }

    public void UpdateFishValue(FishSpecies species, int increaseAmount)
    {
        int index = GetFishIndex(species);
        if (index == -1)
        {
            Debug.Log("Fish not found");
            return;
        }

        // Get the current value and increase it
        int currentValue = PlayerPrefManager.Instance.GetFishValue(species, 0);
        int newValue = currentValue + increaseAmount;

        // Save the new value
        PlayerPrefManager.Instance.SaveFishValue(species, newValue);

        // Update UI elements based on new value
        if (newValue > 0)
        {
            fishNames[index].text = $"{GetFishPluralName(species)} caught:";
            fishNames[index].gameObject.SetActive(true);
            fishScores[index].text = newValue.ToString();
            fishScores[index].gameObject.SetActive(true);
        }
    }

    public void UpdateNonFishValue(SaveValue type, int increaseAmount)
    {
        // Get the current value and increase it
        int currentValue = PlayerPrefManager.Instance.GetPrefValue(type, 0);
        int newValue = currentValue + increaseAmount;

        // Save the new value
        PlayerPrefManager.Instance.SavePrefValue(type, newValue);

        // Update UI elements based on new value
        TMP_Text targetText = type switch
        {
            SaveValue.score => scoreTxt,
            SaveValue.lures_made => lures_made,
            SaveValue.lures_lost => lures_lost,
            SaveValue.decorations => decorations,
            SaveValue.cuts => cuts,
            SaveValue.fishes_missed => fishes_missed,
            _ => null
        };

        if (targetText != null)
        {
            targetText.text = newValue.ToString();
        }
        else
        {
            Debug.LogWarning("Unhandled SaveValue type: " + type);
        }
    }

}
