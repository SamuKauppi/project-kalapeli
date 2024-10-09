using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private bool isFishingMode;
    [SerializeField] private string onTag;          // On tag for when camera is active
    [SerializeField] private string offTag;         // Off tag for when camera is not active

    // Lure stuff
    [SerializeField] private Camera lureCam;
    [SerializeField] private Canvas lureCanvas;

    // Fish stuff
    [SerializeField] private Camera fishingCam;
    [SerializeField] private Canvas fishCanvas;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        UpdateModeChange();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SwapModes();
        }
    }

    private void UpdateModeChange()
    {
        // Lure stuff
        lureCam.gameObject.SetActive(!isFishingMode);
        lureCam.tag = isFishingMode ? offTag : onTag;
        lureCanvas.gameObject.SetActive(!isFishingMode);
        LureCreationManager.Instance.gameObject.SetActive(!isFishingMode);

        // Fish stuff
        fishingCam.gameObject.SetActive(isFishingMode);
        fishingCam.tag = isFishingMode ? onTag : offTag;
        fishCanvas.gameObject.SetActive(isFishingMode);
    }

    public void SwapModes()
    {
        isFishingMode = !isFishingMode;
        UpdateModeChange();
    }
}
