using System.Collections;
using UnityEngine;

/// <summary>
/// Manages camera angles and switching between lure creation and fishing
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Camera MainCamera { get { return mainCam; } }

    [SerializeField] private bool isFishingMode;

    // References
    [SerializeField] private Camera mainCam;
    [SerializeField] private Camera overlayCam;
    [SerializeField] private Canvas lureCanvas;
    [SerializeField] private Canvas fishCanvas;

    // Camera angles
    [SerializeField] private CameraAngle[] lureCameraAngles;
    [SerializeField] private CameraAngle[] fishCameraAngles;

    private int lureAngleID = 0;
    private int fishAngleID = 0;


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
        ChangeCameraAngle(0, 0f);
    }

    private void UpdateModeChange()
    {
        // Lure creation stuff
        LureCreationManager.Instance.gameObject.SetActive(!isFishingMode);
        LureCreationManager.Instance.SetLureCreation(!isFishingMode);

        // Fish manager
        FishManager.Instance.CanFish = isFishingMode;
        if (isFishingMode)
            FishManager.Instance.ActivateFishing();

        // Canvas
        fishCanvas.gameObject.SetActive(isFishingMode);
        lureCanvas.gameObject.SetActive(!isFishingMode);
    }

    private IEnumerator ChangeFov(float targetFov, float changeTime)
    {
        float startFov = mainCam.fieldOfView;
        float time = 0f;

        while (time < changeTime)
        {
            mainCam.fieldOfView = Mathf.Lerp(startFov, targetFov, time / changeTime);
            time += Time.deltaTime;
            yield return null;
        }
        mainCam.fieldOfView = targetFov;
        overlayCam.fieldOfView = targetFov;
    }

    public void SwapModes()
    {
        isFishingMode = !isFishingMode;
        UpdateModeChange();
        int id = !isFishingMode ? lureAngleID : fishAngleID;
        ChangeCameraAngle(id, 0f);
    }

    public void ChangeCameraAngle(int id, float changeTime)
    {
        CameraAngle angle = isFishingMode ? fishCameraAngles[id] : lureCameraAngles[id];

        if (isFishingMode) fishAngleID = id;
        else lureAngleID = id;

        if (LeanTween.isTweening(mainCam.gameObject))
        {
            LeanTween.cancel(mainCam.gameObject);
        }

        if (changeTime > 0 && mainCam.transform.position != angle.posAndRot.position && mainCam.transform.eulerAngles != angle.posAndRot.eulerAngles)
        {
            LeanTween.move(mainCam.gameObject, angle.posAndRot.position, changeTime);
            LeanTween.rotate(mainCam.gameObject, angle.posAndRot.eulerAngles, changeTime);
        }
        else
        {
            mainCam.transform.position = angle.posAndRot.position;
            mainCam.transform.eulerAngles = angle.posAndRot.eulerAngles;
        }
        StartCoroutine(ChangeFov(angle.Fov, changeTime));
    }

    public void SetBothModes(bool value)
    {
        // Enable or disable stuff based on which mode is active
        if (isFishingMode)
        {
            FishManager.Instance.CanFish = value;
            fishCanvas.gameObject.SetActive(value);
        }
        else
        {
            lureCanvas.gameObject.SetActive(value);
            LureCreationManager.Instance.SetLureCreation(value);
        }

    }
}

[System.Serializable]
public class CameraAngle
{
    public Transform posAndRot;
    public float Fov;
}