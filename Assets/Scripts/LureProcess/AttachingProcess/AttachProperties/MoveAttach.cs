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

    [SerializeField] private Outline outline;           // Reference to outline component
    [SerializeField] private float minScale = 0.5f;     // Min scale for attached
    [SerializeField] private float maxScale = 1.5f;     // Max scale for attached

    private int shouldHighlight;                        // Counts if the object should be highlighted
    private Vector3 attachedScale = Vector3.one;
    private float scaleFactor;

    private void OnMouseDown()
    {
        if (!IsMirrored)
        {
            AttachingProcess.Instance.MoveAttached(gameObject, AttachPosition, MatchRotation, MirroredObj);
            AddOutline(1);
        }
    }

    private void OnMouseEnter()
    {
        if (!IsMirrored)
        {
            AddOutline(1);
        }
    }

    private void OnMouseExit()
    {
        if (!IsMirrored)
        {
            AddOutline(-1);
        }
    }

    private void AddOutline(int change)
    {
        if (!AttachingProcess.Instance.IsAttaching) { return; }

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
        AddOutline(value ? 1 : -1);
    }

    public void ScaleAttached(float scale)
    {
        attachedScale = transform.localScale;
        scaleFactor = 1 + scale;

        for (int i = 0; i < 3; i++)
        {
            if (attachedScale[i] > 0)
                attachedScale[i] = Mathf.Clamp(attachedScale[i] * scaleFactor, minScale, maxScale);
            else
                attachedScale[i] = Mathf.Clamp(attachedScale[i] * scaleFactor, -maxScale, -minScale);
        }

        if (TryGetComponent(out ChubProperties cp))
        {
            cp.ScaleChub(attachedScale.x);

        }

        transform.localScale = attachedScale;
    }
}
