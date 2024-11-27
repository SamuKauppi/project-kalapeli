using UnityEngine;

public class FishingLureBox : MonoBehaviour
{
    public static FishingLureBox Instance { get; private set; }

    [SerializeField] private Outline outline;
    [SerializeField] private float onWidth;
    [SerializeField] private float offWidth;
    [SerializeField] private GameObject[] luresInBox;

    // Lure Selector
    [SerializeField] private int selectorEntryCount;
    [SerializeField] private float padding;
    [SerializeField] private GameObject lureSelector;
    [SerializeField] private RectTransform selectorParent;
    [SerializeField] private LureSelectorEntry lureSelectorEntryPrefab;

    private Camera cam;
    private Vector3 mousePos;
    private LureSelectorEntry[] lureSelectorEntries;
    private bool isSelectorOpen;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        lureSelectorEntries = new LureSelectorEntry[selectorEntryCount];

        for (int i = 0; i < lureSelectorEntries.Length; i++)
        {
            lureSelectorEntries[i] = Instantiate(lureSelectorEntryPrefab, selectorParent);
            lureSelectorEntries[i].gameObject.SetActive(false);
        }
        lureSelector.SetActive(false);

        if (outline == null)
            outline = GetComponent<Outline>();

        outline.enabled = false;
        cam = GameManager.Instance.MainCamera;
    }

    private void Update()
    {
        if (isSelectorOpen && FishManager.Instance.CanFish && Input.GetMouseButtonUp(0))
        {
            mousePos = Input.mousePosition;
            mousePos.z = cam.nearClipPlane;
            Vector3 direction = cam.ScreenToWorldPoint(mousePos) - cam.transform.position;

            if (Physics.Raycast(cam.transform.position, direction, out RaycastHit hit, 50f))
            {
                if (hit.collider.gameObject == gameObject || hit.collider.gameObject.layer == 31)
                {
                    return;
                }
            }
            CloseLureSelector();
        }
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
