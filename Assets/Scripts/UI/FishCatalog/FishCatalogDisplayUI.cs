using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class FishCatalogDisplayUI : MonoBehaviour
{
    public TMP_Text nameField;
    public Image imageField;
    public TMP_Text fishingStats;
    public TMP_Text flavourText;

    public void ShowFish(Fish fish)
    {
        nameField.text = fish.Species.ToString();
    }
}
