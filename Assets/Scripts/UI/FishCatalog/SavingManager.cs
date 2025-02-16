using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles saving player prefs and displaying them into catalog
/// </summary>
public class SavingManager : MonoBehaviour
{
    public static SavingManager Instance { get; private set; }

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
        ReloadCatalogTexts();
    }

    private void ReloadCatalogTexts()
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

    private TMP_Text GetTextValue(SaveValue type)
    {
        // Update UI elements based on new value
        return type switch
        {
            SaveValue.score => scoreTxt,
            SaveValue.lures_made => lures_made,
            SaveValue.lures_lost => lures_lost,
            SaveValue.decorations => decorations,
            SaveValue.cuts => cuts,
            SaveValue.fishes_missed => fishes_missed,
            _ => null
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

        TMP_Text targetText = GetTextValue(type);

        if (targetText != null)
        {
            targetText.text = newValue.ToString();
        }
        else
        {
            Debug.LogWarning("Unhandled SaveValue type: " + type);
        }
    }

    public void ResetSaveData()
    {
        PersitentManager.Instance.ForgetFishesCaught();
        foreach (SaveValue save in Enum.GetValues(typeof(SaveValue)))
        {
            if (save != SaveValue.music_volume 
                && save != SaveValue.ambient_volume 
                && save != SaveValue.sfx_volume)
            {
                PlayerPrefManager.Instance.SavePrefValue(save, 0);
            }
        }
        ReloadCatalogTexts();
    }

    public void ShowEndGame()
    {
        FishCatalog.Instance.CloseCatalog();
    }

    public void CatchEveryFish()
    {
        foreach (FishSpecies fish in Enum.GetValues(typeof(FishSpecies)))
        {
            if (fish != FishSpecies.None)
            {
                UpdateFishValue(fish, 1);
                PersitentManager.Instance.AddCaughtFish(fish);
            }
        }
    }
}
