using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class FishCatalogDisplayUI : MonoBehaviour
{
    public TMP_Text nameField;
    public Image imageField;
    public TMP_Text fishingStats;
    public TMP_Text flavourText;

    public void ShowFish(Fish fish, bool isCaught)
    {
        fishingStats.text = fish.HintText;
        if (!isCaught)
        {
            nameField.text = "Unknown Fish";
            flavourText.text = "";
        }
        else
        {
            nameField.text = fish.Species.ToString();
            flavourText.text = fish.FlavourText;
        }

    }
}
