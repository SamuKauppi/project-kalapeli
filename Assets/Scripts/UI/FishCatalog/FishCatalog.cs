using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used to display fishcatalog
/// </summary>
public class FishCatalog : MonoBehaviour
{
    public static FishCatalog Instance { get; private set; }

    // Page selection
    [SerializeField] private GameObject fishPage;
    [SerializeField] private GameObject scorePage;
    [SerializeField] private GameObject settingsPage;
    private int currentPageType;

    // Catalog
    [SerializeField] private GameObject catalogUI;          // Object moved when opening or closing catalog
    [SerializeField] private Transform cataOnPos;           // When active pos
    [SerializeField] private Transform cataOffPos;          // When closed pos
    [SerializeField] private GameObject closeButton;        // Button to close catalog
    [SerializeField] private float displaySpeed = 0.35f;    // How fast is the catalog display
    [SerializeField] private LeanTweenType ease1;           // Ease type for the animation when opening
    [SerializeField] private LeanTweenType ease2;           // Ease type for the animation when closing
    private bool isCatalogOpen = false;                     // Is catalog open or closed

    // Entry display
    [SerializeField] private Sprite defaultMiniSprite;
    [SerializeField] private FishCatalogUIEntry[] entries;  // Fish entries
    [SerializeField] private Button pageForward;            // Next page
    [SerializeField] private Button pageBackward;           // Previous page
    private Fish[] everyFish;                               // Every fish in game
    private readonly List<List<Fish>> fishCatalog = new();  // Fish catalog
    private int pageNumber = 0;                             // Which page is currently displayed

    // Fish display
    [SerializeField] private FishCatalogDisplayUI displayer;
    private FishCatalogUIEntry previousEntry = null;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        displayer.gameObject.SetActive(false);
        SetCatalog(isCatalogOpen, 0f);

        everyFish = PersitentManager.Instance.EveryFish;
        int entryNumber = 0;
        foreach (Fish fish in everyFish)
        {
            if (entryNumber > 8)
            {
                entryNumber = 0;
            }

            if (entryNumber == 0)
            {
                fishCatalog.Add(new());
            }

            fishCatalog[^1].Add(fish);
            entryNumber++;
        }
    }

    /// <summary>
    /// Sets catalog open or closed
    /// </summary>
    /// <param name="value"></param>
    /// <param name="time"></param>
    private void SetCatalog(bool value, float time)
    {
        Vector3 targetPos = value
            ? cataOnPos.position
            : cataOffPos.position;


        LeanTweenType ease = value ? ease1 : ease2;

        if (LeanTween.isTweening(catalogUI))
        {
            LeanTween.cancel(catalogUI);
        }
        LeanTween.move(catalogUI, targetPos, time).setEase(ease);

        if (GameManager.Instance)
            GameManager.Instance.SetBothModes(!value);

        ShowCatalog(0);
        closeButton.SetActive(value);
    }

    public void OpenCatalogPage(int pageType)
    {
        SoundManager.Instance.PlaySound(SoundClipTrigger.OnOpenBook);

        if (Tutorial.Instance)
            Tutorial.Instance.TutorialOpenCatalog();

        if (!isCatalogOpen)
        {
            isCatalogOpen = true;
            SetCatalog(isCatalogOpen, displaySpeed);
        }
        else if (pageType == currentPageType)
        {
            CloseCatalog();
        }

        currentPageType = pageType;
        fishPage.SetActive(currentPageType == 0);
        scorePage.SetActive(currentPageType == 1);
        settingsPage.SetActive(currentPageType == 2);
    }

    public void CloseCatalog()
    {
        isCatalogOpen = false;
        SetCatalog(isCatalogOpen, displaySpeed);
        SoundManager.Instance.PlaySound(SoundClipTrigger.OnCloseBook);

        if(Tutorial.Instance)
            Tutorial.Instance.TutorialCloseCatalog();
    }

    /// <summary>
    /// Shows the entries for this page in catalog
    /// </summary>
    /// <param name="page"></param>
    public void ShowCatalog(int page)
    {
        if (page < 0 || page >= fishCatalog.Count) { return; }

        var currentPage = fishCatalog[page];
        for (int i = 0; i < entries.Length; i++)
        {
            bool isEntryActive = i < currentPage.Count;
            entries[i].gameObject.SetActive(isEntryActive);

            if (isEntryActive)
            {
                Sprite sprite = PersitentManager.Instance.IsFishCaught(currentPage[i].Species) ?
                    currentPage[i].miniIcon : defaultMiniSprite;

                entries[i].DefineFishToDisplay(currentPage[i].Species, sprite);
            }
        }

        pageNumber = page;
        pageBackward.interactable = pageNumber > 0;
        pageForward.interactable = pageNumber < fishCatalog.Count - 1;
    }

    /// <summary>
    /// Moves the page by value
    /// </summary>
    /// <param name="direction"></param>
    public void ChangePage(int direction)
    {
        ShowCatalog(pageNumber + direction);
    }

    /// <summary>
    /// Diplays fish details in catalog
    /// </summary>
    /// <param name="fish"></param>
    public void DisplayFish(FishSpecies fish)
    {
        for (int i = 0; i < everyFish.Length; i++)
        {
            if (everyFish[i].Species == fish)
            {
                displayer.gameObject.SetActive(true);
                displayer.ShowFish(everyFish[i], PersitentManager.Instance.IsFishCaught(everyFish[i].Species));
                if (previousEntry != null)
                {
                    previousEntry.SetSelected(false);
                }
                entries[i].SetSelected(true);
                previousEntry = entries[i];
                return;
            }
        }

        displayer.gameObject.SetActive(false);
        if (previousEntry != null)
        {
            previousEntry.SetSelected(false);
            previousEntry = null;
        }
    }
}
