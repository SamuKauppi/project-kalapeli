using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private bool isFishingMode;
    [SerializeField] private string onTag;          // On tag for when camera is active
    [SerializeField] private string offTag;         // Off tag for when camera is not active

    // Lure stuff
    public Camera LureCamera { get { return lureCam; } }
    [SerializeField] private Camera lureCam;
    [SerializeField] private Canvas lureCanvas;

    // Fish stuff
    public Camera FishCamera { get { return fishCam; } }
    [SerializeField] private Camera fishCam;
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

    private void UpdateModeChange()
    {
        // Lure stuff
        lureCam.gameObject.SetActive(!isFishingMode);
        lureCam.tag = isFishingMode ? offTag : onTag;
        lureCanvas.gameObject.SetActive(!isFishingMode);
        LureCreationManager.Instance.gameObject.SetActive(!isFishingMode);

        // Fish stuff
        fishCam.gameObject.SetActive(isFishingMode);
        fishCam.tag = isFishingMode ? onTag : offTag;
        fishCanvas.gameObject.SetActive(isFishingMode);

        // Other stuff
        if (!isFishingMode)
        {
            LureCreationManager.Instance.ResumeLureCreation();
        }
    }

    public void SwapModes()
    {
        isFishingMode = !isFishingMode;
        UpdateModeChange();
    }
}
