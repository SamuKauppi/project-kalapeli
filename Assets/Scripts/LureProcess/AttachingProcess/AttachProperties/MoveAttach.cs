using UnityEngine;

/// <summary>
/// Starts moving already attached object
/// Stores data from when it was created for other scripts
/// </summary>
public class MoveAttach : MonoBehaviour
{
    public GameObject MirroredObj { get; set; }         // Reference to mirrored object
    public AttachPosition AttachPosition { get; set; }  // Which attach position was used
    public bool MatchRotation { get; set; }             // Was the rotation matched
    public bool IsMirrored { get; set; } = false;       // Is this object mirrored

    [SerializeField] private Outline outline;   // Reference to outline component
    private int shouldHighlight;                // Counts if the object should be highlighted

    private void OnMouseDown()
    {
        // Check if the attaching process is active
        if (!AttachingProcess.Instance.IsAttaching) { return; }

        if (!IsMirrored)
        {
            AttachingProcess.Instance.MoveAttached(gameObject, AttachPosition, MatchRotation, MirroredObj);
            AddOutline(1);
        }
    }

    private void OnMouseEnter()
    {
        // Check if the attaching process is active
        if (!AttachingProcess.Instance.IsAttaching) { return; }

        if (!IsMirrored)
        {
            AddOutline(1);
        }
    }

    private void OnMouseExit()
    {
        // Check if the attaching process is active
        if (!AttachingProcess.Instance.IsAttaching) { return; }

        if (!IsMirrored)
        {
            AddOutline(-1);
        }
    }

    private void AddOutline(int change)
    {
        shouldHighlight += change;
        if (shouldHighlight > 0)
        {
            outline.enabled = true;
        }
        else
        {
            outline.enabled = false;
        }
    }

    public void EnableOutline(bool value)
    {
        // Check if the attaching process is active
        if (!AttachingProcess.Instance.IsAttaching) { return; }

        AddOutline(value ? 1 : -1);
    }

}
