using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Handles switching from cutting to attaching process
/// </summary>
public class LureCreationManager : MonoBehaviour
{
    public static LureCreationManager Instance { get; private set; }

    private enum LureCreationProcess
    {
        None,
        Cutting,
        Painting,
        Attaching,
        Saving
    }
    private LureCreationProcess _process;

    // References
    [SerializeField] private DrawCut cutProcess;                // Cut process
    [SerializeField] private BlockPainter paintProcess;         // Painting process
    [SerializeField] private AttachingProcess attachProcess;    // Attach process
    [SerializeField] private GameObject lureObject;             // Ref to lure object

    // Buttons
    [SerializeField] private GameObject cuttingButtons;
    [SerializeField] private GameObject paintingButtons;
    [SerializeField] private GameObject attachButtons;
    [SerializeField] private GameObject saveButtons;
    [SerializeField] private GameObject warningPopUp;
    [SerializeField] private GameObject backToFish;
    [SerializeField] private GameObject rotateButtons1;
    [SerializeField] private GameObject rotateButtons2;

    // Mesh reset
    [SerializeField] private Mesh blockMesh;
    [SerializeField] private Material blockMaterial;
    [SerializeField] private PhysicMaterial physicMaterial;

    private MeshRenderer blockRend;
    private MeshFilter blockFilter;
    private LureFunctions lureProperties;

    #region private
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        blockRend = BlockRotation.Instance.GetComponent<MeshRenderer>();
        blockFilter = BlockRotation.Instance.GetComponent<MeshFilter>();
        lureProperties = BlockRotation.Instance.GetComponent<LureFunctions>();
        Cutter.Instance.RecalculateUvs(blockMesh);
        ResetLureCreation();
        warningPopUp.SetActive(false);
    }

    private void ResetToCuttingBlock()
    {
        // Reset mesh
        blockFilter.mesh = blockMesh;

        ResetBlockMaterial();

        // Reset collider
        MeshCollider col = lureObject.GetComponent<MeshCollider>();
        col.sharedMaterial = physicMaterial;
        col.sharedMesh = blockMesh;
        col.convex = true;

        // Reset lure properties (remove children and reset stats)
        lureProperties.ResetLure();
    }

    private void ResetBlockMaterial()
    {
        // Reset materials
        Material[] mats = new Material[1];
        mats[0] = blockMaterial;
        blockRend.materials = mats;
    }

    private void SetCutting(bool value)
    {
        // Enable cutting
        cutProcess.IsCutting = value;
        cutProcess.gameObject.SetActive(value);
        cuttingButtons.SetActive(value);
    }
    private void SetPainting(bool value)
    {
        // Disable painting
        paintingButtons.SetActive(value);
        if (value)
            paintProcess.Activate();
    }
    private void SetAttaching(bool value)
    {
        // Disable attaching
        attachButtons.SetActive(value);
        attachProcess.IsAttaching = value;
        if (value)
            attachProcess.ActivateAttaching();
    }
    private void SetSaving(bool value)
    {
        // Disable saving
        saveButtons.SetActive(value);
    }

    private void ResetBlockRotation(float timeToReset)
    {
        // Reset rotation
        BlockRotation.Instance.ResetRotation(timeToReset);
        BlockRotation.Instance.StopRotating = false;    // Ensure the block can be rotated
    }
    private IEnumerator ActivateAfterCameraAngle(int index, float time, Action[] callBacks)
    {
        GameManager.Instance.ChangeCameraAngle(index, time);
        yield return new WaitForSeconds(time);
        yield return null;
        foreach (Action action in callBacks)
        {
            action?.Invoke();
        }
    }

    private void SetMode(LureCreationProcess process)
    {
        // Change mode
        _process = process;
        SetCutting(process == LureCreationProcess.Cutting);
        SetPainting(process == LureCreationProcess.Painting);
        SetAttaching(process == LureCreationProcess.Attaching);
        SetSaving(process == LureCreationProcess.Saving);
        BlockRotation.Instance.StopRotating = false;    // Ensure the block can be rotated

        // Set rotate buttons
        rotateButtons1.SetActive(_process != LureCreationProcess.Attaching);
        rotateButtons2.SetActive(_process == LureCreationProcess.Attaching);

    }

    #endregion
    #region public

    public void ResetLureCreation()
    {
        // Change camera angle
        GameManager.Instance.ChangeCameraAngle(0, 0f);

        // Reset the rotation of block
        ResetBlockRotation(0f);

        // Reset mesh
        ResetToCuttingBlock();

        // Set mode to cutting
        SetMode(LureCreationProcess.Cutting);
    }
    public void CancelWarningPopUp()
    {
        SetMode(_process);
        warningPopUp.SetActive(false);
        backToFish.SetActive(true);
    }
    public void ReEnterCutting(bool wasFromWarning)
    {
        if (!wasFromWarning && lureProperties.transform.childCount > 1)
        {
            // Disable every mode separately as _process should not be changed
            SetCutting(false);
            SetPainting(false);
            SetAttaching(false);
            SetSaving(false);
            backToFish.SetActive(false);
            rotateButtons1.SetActive(false);
            rotateButtons2.SetActive(false);
            // Enable popup
            warningPopUp.SetActive(true);
            // Diable rotation
            BlockRotation.Instance.StopRotating = true;
            return;
        }

        warningPopUp.SetActive(false);
        backToFish.SetActive(true);

        // Change camera angle
        var callBacks = new Action[] {
            () => ResetBlockRotation(0.1f)
        };

        StartCoroutine(ActivateAfterCameraAngle(0, 0.5f, callBacks));

        // Reset only material for block
        ResetBlockMaterial();

        // Reset lure properties
        lureProperties.ResetLure();

        // Set mode to cutting
        SetMode(LureCreationProcess.Cutting);
    }

    public void StartPainting()
    {
        // Change Camera angle
        GameManager.Instance.ChangeCameraAngle(0, 0.5f);

        // Set mode to painting
        SetMode(LureCreationProcess.Painting);

    }

    public void StartAttaching()
    {
        // Change Camera angle and after set mode
        var callBacks = new Action[]
        {
            () => ResetBlockRotation(0.15f),
            () => SetMode(LureCreationProcess.Attaching)
        };
        StartCoroutine(ActivateAfterCameraAngle(1, 0.5f, callBacks));
    }

    public void StartSaving()
    {
        if (!lureProperties.CanCatch) { return; }

        // Set camera angle and update mode
        var callBacks = new Action[]
        {
            () => ResetBlockRotation(0.15f)
        };
        lureProperties.FinalizeLure();
        StartCoroutine(ActivateAfterCameraAngle(0, 0.5f, callBacks));
        SetMode(LureCreationProcess.Saving);
    }

    public void SaveLure()
    {
        SetMode(LureCreationProcess.None);
        BlockRotation.Instance.ResetRotation(0f);
        PersitentManager.Instance.AddNewLure(lureObject);
    }

    public void EndLureCreation()
    {
        BlockRotation.Instance.StopRotating = true;
        GameManager.Instance.SwapModes();
    }

    public void SetLureCreation(bool value)
    {
        BlockRotation.Instance.StopRotating = !value;
        if (_process == LureCreationProcess.Cutting)
        {
            cutProcess.GetComponent<DrawCut>().IsCutting = value;
        }
        else if (_process == LureCreationProcess.Attaching)
        {
            attachProcess.GetComponent<AttachingProcess>().IsAttaching = value;
        }
    }
    #endregion
}
