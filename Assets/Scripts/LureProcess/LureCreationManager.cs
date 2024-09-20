using UnityEngine;

public class LureCreationManager : MonoBehaviour
{
    // References
    [SerializeField] private DrawCut cutProcess;                // Cutprocess
    [SerializeField] private AttachingProcess attachProcess;    // Attach process
    [SerializeField] private GameObject lureObject;             // Ref to lure object

    private void Start()
    {
        cutProcess.IsCutting = true;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
#if UNITY_EDITOR
            SaveAsset.SaveGameObjectAsPrefab(lureObject);
#endif
        }
    }


    public void EndCutting()
    {
        cutProcess.IsCutting = false;
        attachProcess.IsAttaching = true;

        // Ensure that the block can be rotated after cutting ends
        lureObject.GetComponent<BlockRotation>().StopRotating = false;
    }
}
