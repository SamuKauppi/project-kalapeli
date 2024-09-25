using UnityEngine;

/// <summary>
/// Starts moving already attached object
/// Stores data from when it was created
/// </summary>
public class MoveAttach : MonoBehaviour
{
    public GameObject MirroredObj {  get; set; }        // Reference to mirrored object
    public AttachPosition AttachPosition { get; set; }  // Which attach position was used
    public bool MatchRotation { get; set; }             // Was the rotation matched
    public bool IsMirrored { get; set; } = false;       // Is this object mirrored


    private void OnMouseDown()
    {
        if (!IsMirrored)
            AttachingProcess.Instance.MoveAttached(gameObject, AttachPosition, MatchRotation, MirroredObj);
    }
}
