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

    private void Start()
    {
        cutProcess.IsCutting = true;
        cuttingButtons.SetActive(true);
        paintingButtons.SetActive(false);
        attachButtons.SetActive(false);
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
        cutProcess.IsCutting = false;
        cutProcess.gameObject.SetActive(false);

        cuttingButtons.SetActive(false);
        paintProcess.Activate();
        paintingButtons.SetActive(true);

        // Ensure that the block can be rotated after cutting ends
        lureObject.GetComponent<BlockRotation>().StopRotating = false;
    }

    public void EndPainting()
    {
        paintingButtons.SetActive(false);
        paintProcess.gameObject.SetActive(false);
        attachButtons.SetActive(true);

        attachProcess.ActivateAttaching();

        // Ensure that the block can be rotated after cutting ends
        lureObject.GetComponent<BlockRotation>().StopRotating = false;
    }
}
