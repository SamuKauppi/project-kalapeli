using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public static Tutorial Instance { get; private set; }

    // Notice me exclamation sign
    [SerializeField] private UIColorAnimation noticeMe;
    [SerializeField] private RectTransform noticeMeTransform;
    [SerializeField] private RectTransform[] noticeMePositions;

    // Cutting animation
    [SerializeField] private CuttingAnimation cuttingAnim;

    private int tutorialStep = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        tutorialStep = PlayerPrefManager.Instance.GetPrefValue(SaveValue.tutorial, 0);
        noticeMe.SetImageActive(false);

        if (tutorialStep == 0)
        {
            noticeMe.SetImageActive(true);
            noticeMeTransform.anchoredPosition = noticeMePositions[0].anchoredPosition;
            AddToTutorial();
        }
    }


    private void OnEnable()
    {
        GameManager.OnModeChange += DetectModeChange;
        BlockRotation.OnRotationStart += DetectRotation;
    }

    private void OnDisable()
    {
        GameManager.OnModeChange -= DetectModeChange;
        BlockRotation.OnRotationStart -= DetectRotation;
    }

    private void DetectModeChange(bool isFishing)
    {
        switch (tutorialStep)
        {
            case 1:
                AddToTutorial();
                cuttingAnim.ScaleTowards();
                noticeMe.SetImageActive(false);
                break;
            case 2:
                break;
            case 3:
                noticeMe.SetImageActive(false);
                AddToTutorial();
                break;
            default:
                break;
        }
    }

    private void DetectRotation(int side, int up)
    {
        if (side == 0 && up == 0) return;

        if (noticeMeTransform.anchoredPosition == noticeMePositions[1].anchoredPosition && tutorialStep > 1)
        {
            noticeMe.SetImageActive(false);
            AddToTutorial();
        }
    }

    private void AddToTutorial(int value = 1)
    {
        tutorialStep += value;
        PlayerPrefManager.Instance.SavePrefValue(SaveValue.tutorial, tutorialStep);
    }

    public void EndCuttingTutorial()
    {
        if (tutorialStep == 2)
        {
            AddToTutorial();
            cuttingAnim.EndAnimation();
            noticeMe.SetImageActive(true);
            noticeMeTransform.anchoredPosition = noticeMePositions[1].anchoredPosition;
        }
    }

    public void CatchNewFish()
    {
        noticeMe.SetImageActive(true);
        noticeMeTransform.position = noticeMePositions[2].position;
    }

    public void CheckNewFish()
    {
        if (tutorialStep > 2)
            noticeMe.SetImageActive(false);
    }
}
