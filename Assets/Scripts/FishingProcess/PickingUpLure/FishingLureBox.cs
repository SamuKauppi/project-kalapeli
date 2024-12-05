using UnityEngine;

/// <summary>
/// Handles picking up lures and the box
/// </summary>
public class FishingLureBox : MonoBehaviour
{
    public static FishingLureBox Instance { get; private set; }

    // Outline
    [SerializeField] private Outline outline;
    [SerializeField] private float onWidth;
    [SerializeField] private float offWidth;

    // Purely visual objects in box
    [SerializeField] private GameObject[] luresInBox;

    // Lure Selector (world space UI)
    [SerializeField] private GameObject lureSelector;                   // Refernece to UI object
    [SerializeField] private RectTransform selectorParent;              // Where contetn is created
    [SerializeField] private LureSelectorEntry lureSelectorEntryPrefab; // Refernece to prefab
    [SerializeField] private int selectorEntryCount;                    // How many lures can be displayed
    [SerializeField] private float padding;                             // Padding for each UI element
    private LureSelectorEntry[] lureSelectorEntries;       // Empty entries created during runtime
    private bool isSelectorOpen;                           // Is selector open

    // Private
    private Camera cam;         // Camera
    private Vector3 mousePos;   // Mouse Position

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Create entries
        GenerateLureSelecotrEntries();

        // Set outline
        outline = GetComponent<Outline>();
        outline.enabled = false;

        // Get camera
        cam = GameManager.Instance.MainCamera;
    }

    /// <summary>
    /// Generates entries for lureselector
    /// </summary>
    private void GenerateLureSelecotrEntries()
    {
        lureSelectorEntries = new LureSelectorEntry[selectorEntryCount];

        for (int i = 0; i < lureSelectorEntries.Length; i++)
        {
            lureSelectorEntries[i] = Instantiate(lureSelectorEntryPrefab, selectorParent);
            lureSelectorEntries[i].gameObject.SetActive(false);
        }
        lureSelector.SetActive(false);
    }

    private void Update()
    {
        // Try to close lure selecor when clicked somewhere outside of some objects
        if (isSelectorOpen && FishManager.Instance.CanFish && Input.GetMouseButtonUp(0))
        {
            CheckIfSelectorCanClose();
        }
    }

    private void CheckIfSelectorCanClose()
    {
        mousePos = Input.mousePosition;
        mousePos.z = cam.nearClipPlane;
        Vector3 direction = cam.ScreenToWorldPoint(mousePos) - cam.transform.position;

        if (Physics.Raycast(cam.transform.position, direction, out RaycastHit hit, 50f))
        {
            // If the hit is this object or it's on DrawFront layer (LureTrash is on this layer)
            if (hit.collider.gameObject == gameObject || hit.collider.gameObject.layer == 31)
            {
                return;
            }
        }
        CloseLureSelector();
    }

    private void OnMouseDown()
    {
        if (!isSelectorOpen && FishManager.Instance.CanFish)
            OpenLureSelector();
    }

    private void OnMouseEnter()
    {
        if (!isSelectorOpen && FishManager.Instance.CanFish)
            outline.OutlineWidth = onWidth;
    }

    private void OnMouseExit()
    {
        outline.OutlineWidth = offWidth;
    }

    private void OpenLureSelector()
    {
        isSelectorOpen = true;
        lureSelector.SetActive(true);
        LureStats[] lures = PersitentManager.Instance.GetLureArray();
        selectorParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
            lures.Length * (lureSelectorEntryPrefab.entryBtn.image.rectTransform.rect.height + padding));

        for (int i = 0; i < lureSelectorEntries.Length; i++)
        {
            if (i <= lures.Length - 1)
            {
                lureSelectorEntries[i].gameObject.SetActive(true);
                lureSelectorEntries[i].lureNameField.text = lures[i].GetComponent<LureStats>().lureName;
                lureSelectorEntries[i].SetTargetForEntry(lures[i].gameObject);
            }
            else
            {
                lureSelectorEntries[i].gameObject.SetActive(false);
            }
        }
    }

    public void CloseLureSelector()
    {
        isSelectorOpen = false;
        lureSelector.SetActive(false);
    }

    public void SetLureBoxActive(int lureCount)
    {
        outline.enabled = lureCount > 0;
        for (int i = 0; i < luresInBox.Length; i++)
        {
            luresInBox[i].SetActive(lureCount > 0);
        }
    }
}
