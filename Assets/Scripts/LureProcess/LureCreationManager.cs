using UnityEngine;

/// <summary>
/// Handles switching from cutting to attaching process
/// </summary>
public class LureCreationManager : MonoBehaviour
{
    public static LureCreationManager Instance { get; private set; }

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
    private LureProperties lureProperties;

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
        lureProperties = BlockRotation.Instance.GetComponent<LureProperties>(); 
        ResetLureCreation();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {

            SaveAsset.SaveGameObjectAsPrefab(lureObject);
        }
    }
#endif

    public void ResetLureCreation()
    {
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
        if (lureProperties.SwimType == SwimmingType.Bad)
            return;

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
        if (lureProperties.SwimType == SwimmingType.Bad)
            return;

        // Hide attach buttons and reveal save buttons
        attachButtons.SetActive(false);
        saveButtons.SetActive(true);

        // Disable attaching
        attachProcess.IsAttaching = false;
        attachProcess.gameObject.SetActive(false);

        // Ensure that the block can be rotated
        BlockRotation.Instance.StopRotating = false;
    }

    public void SaveLure()
    {
        saveButtons.SetActive(false);
        PersitentManager.Instance.AddLure(lureObject);
    }

    public void EndLureCreation()
    {
        BlockRotation.Instance.StopRotating = true;
        GameManager.Instance.SwapModes();
    }

    public void ResumeLureCreation()
    {
        BlockRotation.Instance.StopRotating = false;
    }
}
