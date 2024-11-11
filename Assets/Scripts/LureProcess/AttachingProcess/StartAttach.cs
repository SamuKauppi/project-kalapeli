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
        }
    }

    private void OnMouseEnter()
    {
        if (outline != null)
            outline.enabled = true;
    }
    private void OnMouseExit()
    {
        if (outline != null)
            outline.enabled = false;
    }

    private void OnMouseDown()
    {
        AttachingProcess.Instance.StartAttachingObject(objectToAttach);
    }
}
