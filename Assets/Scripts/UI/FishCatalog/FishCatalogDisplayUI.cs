using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class FishCatalogDisplayUI : MonoBehaviour
{
    // Pages
    [SerializeField] private GameObject[] pages;
    private int pageIndex;

    // Display fields
    [SerializeField] private TMP_Text nameField;
    [SerializeField] private Image imageField;
    [SerializeField] private Image depthField;
    [SerializeField] private Image foodField;
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private TMP_Text flavourText;
    [SerializeField] private Sprite defaultBigSprite;

    private const string NAME_UNKOWN = "Unknown Fish";
    private const string FISH_UNKOWN = "You'll need to catch this fish to uncover its secrets!";

    private void SetPageVisibility()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (i == pageIndex)
                pages[i].SetActive(true);
            else
                pages[i].SetActive(false);
        }
    }

    public void ShowFish(Fish fish, bool isCaught)
    {
        hintText.text = fish.HintText;
        depthField.sprite = fish.depthIcon;
        foodField.sprite = fish.foodIcon;

        pageIndex = !isCaught ? 0 : 1;

        SetPageVisibility();

        if (!isCaught)
        {
            nameField.text = NAME_UNKOWN;
            flavourText.text = FISH_UNKOWN;
            imageField.sprite = defaultBigSprite;
        }
        else
        {
            nameField.text = fish.Species.ToString();
            flavourText.text = fish.FlavourText;
            imageField.sprite = fish.bigIcon;
        }

        SoundManager.Instance.PlaySound(SoundClipTrigger.OnOpenBook);
    }

    public void ChangePage(int change)
    {
        pageIndex = (pageIndex + change + pages.Length) % pages.Length;
        SetPageVisibility();
    }

}
