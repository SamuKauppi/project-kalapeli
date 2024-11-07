using UnityEngine;

/// <summary>
/// Handles switching from cutting to attaching process
/// </summary>
public class LureCreationManager : MonoBehaviour
{
    public static LureCreationManager Instance { get; private set; }

    private enum LureCreationProcess
    {
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

    // Mesh reset
    [SerializeField] private Mesh blockMesh;
    [SerializeField] private Material blockMaterial;
    [SerializeField] private PhysicMaterial physicMaterial;

    private MeshRenderer blockRend;
    private MeshFilter blockFilter;
    private LureFunctions lureProperties;

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
        ResetLureCreation();
    }

    public void ResetLureCreation()
    {
        _process = LureCreationProcess.Cutting;

        // Enable cutting
        cutProcess.IsCutting = true;
        cutProcess.gameObject.SetActive(true);
        cuttingButtons.SetActive(true);

        // Reset mesh
        blockFilter.mesh = blockMesh;

        // Reset materials
        Material[] mats = new Material[1];
        mats[0] = blockMaterial;
        blockRend.materials = mats;

        // Reset collider
        MeshCollider col = lureObject.GetComponent<MeshCollider>();
        col.sharedMaterial = physicMaterial;
        col.sharedMesh = blockMesh;
        col.convex = true;

        // Reset lure properties (remove children and reset stats)
        lureProperties.ResetLure();

        // Disable painting
        paintingButtons.SetActive(false);

        // Disable attaching
        attachButtons.SetActive(false);
        attachProcess.IsAttaching = false;
        attachProcess.gameObject.SetActive(true);   // Keep it visible

        // Disable saving
        saveButtons.SetActive(false);

        // Ensure that the block can be rotated
        BlockRotation.Instance.StopRotating = false;
    }

    public void EndCutting()
    {
        if (lureProperties.Stats.SwimType == SwimmingType.Bad)
            return;

        _process = LureCreationProcess.Painting;

        // Hide cutting buttons and reveal painting buttons
        cuttingButtons.SetActive(false);
        paintingButtons.SetActive(true);

        // Disable cutting
        cutProcess.IsCutting = false;
        cutProcess.gameObject.SetActive(false);

        // Activate painting
        paintProcess.Activate();

        // Ensure that the block can be rotated
        BlockRotation.Instance.StopRotating = false;
    }

    public void EndPainting()
    {
        _process = LureCreationProcess.Attaching;

        // Hide painting buttons and reveal attach buttons
        paintingButtons.SetActive(false);
        attachButtons.SetActive(true);

        // Disable painting
        paintProcess.gameObject.SetActive(false);

        // Enable attaching
        attachProcess.ActivateAttaching();

        // Ensure that the block can be rotated
        BlockRotation.Instance.StopRotating = false;
    }

    public void EndAttaching()
    {
        // Don't end attaching if swimstyle is not valid
        if (lureProperties.Stats.SwimType == SwimmingType.Bad ||
            lureProperties.Stats.SwimType == SwimmingType.None ||
            !lureProperties.CanCatch)
            return;

        _process = LureCreationProcess.Saving;

        // Hide attach buttons and reveal save buttons
        attachButtons.SetActive(false);
        saveButtons.SetActive(true);

        // Disable attaching
        attachProcess.IsAttaching = false;
        attachProcess.gameObject.SetActive(false);

        // Ensure that the block can be rotated
        BlockRotation.Instance.StopRotating = false;
        lureProperties.FinalizeLure();
    }

    public void SaveLure()
    {
        saveButtons.SetActive(false);
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
}
