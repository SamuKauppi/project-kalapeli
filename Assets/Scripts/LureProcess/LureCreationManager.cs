using UnityEngine;

/// <summary>
/// Handles switching from cutting to attaching process
/// </summary>
public class LureCreationManager : MonoBehaviour
{
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

    private void Start()
    {
        cutProcess.IsCutting = true;
        cuttingButtons.SetActive(true);
        paintingButtons.SetActive(false);
        attachButtons.SetActive(false);
        saveButtons.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
#if UNITY_EDITOR
            SaveAsset.SaveGameObjectAsPrefab(lureObject);
#endif
        }
    }

    public void EndCutting()
    {
        // Hide cutting buttons and reveal painting buttons
        cuttingButtons.SetActive(false);
        paintingButtons.SetActive(true);

        // Disable cutting
        cutProcess.IsCutting = false;
        cutProcess.gameObject.SetActive(false);

        // Activate painting
        paintProcess.Activate();

        // Ensure that the block can be rotated
        lureObject.GetComponent<BlockRotation>().StopRotating = false;
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
        lureObject.GetComponent<BlockRotation>().StopRotating = false;
    }

    public void EndAttaching()
    {
        // Hide attach buttons and reveal save buttons
        attachButtons.SetActive(false);
        saveButtons.SetActive(true);

        // Disable attaching
        attachProcess.IsAttaching = false;
        attachProcess.gameObject.SetActive(false);

        // Ensure that the block can be rotated
        lureObject.GetComponent<BlockRotation>().StopRotating = false;
    }

    public void SaveLure()
    {
        saveButtons.SetActive(false);
        PersitentManager.Instance.AddLure(lureObject);
    }
}
