using UnityEngine;

/// <summary>
/// Starts attaching a object based on AttachingType
/// </summary>
public class StartAttach : MonoBehaviour
{
    [SerializeField] private AttachingType objectToAttach;
    private Outline outline;
    private void Start()
    {
        if (TryGetComponent<Outline>(out var line))
        {
            outline = line;
            line.enabled = false;
        }
    }

    private void OnMouseEnter()
    {
        if (outline != null && AttachingProcess.Instance.IsAttaching)
            ShowOutline(true);
    }
    private void OnMouseExit()
    {
        if (outline != null && AttachingProcess.Instance.IsAttaching)
            ShowOutline(false);
    }

    private void ShowOutline(bool value)
    {
        outline.enabled = value;
    }

    virtual protected void OnMouseDown()
    {
        if (AttachingProcess.Instance.IsAttaching)
            AttachingProcess.Instance.StartAttachingObject(objectToAttach);
    }
}
