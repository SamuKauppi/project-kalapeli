using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class FishCatalogDisplayUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameField;
    [SerializeField] private Image imageField;
    [SerializeField] private TMP_Text fishingStats;
    [SerializeField] private TMP_Text flavourText;
    [SerializeField] private Sprite defaultBigSprite;

    public void ShowFish(Fish fish, bool isCaught)
    {
        fishingStats.text = fish.HintText;
        if (!isCaught)
        {
            nameField.text = "Unknown Fish";
            flavourText.text = "";
            imageField.sprite = defaultBigSprite;
        }
        else
        {
            nameField.text = fish.Species.ToString();
            flavourText.text = fish.FlavourText;
            imageField.sprite = fish.bigIcon;
        }
    }
}
