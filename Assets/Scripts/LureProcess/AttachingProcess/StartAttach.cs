using UnityEngine;

/// <summary>
/// Starts attaching a object based on AttachingType
/// </summary>
public class StartAttach : MonoBehaviour
{
    [SerializeField] private AttachingType objectToAttach;

    private void OnMouseDown()
    {
        AttachingProcess.Instance.StartAttachingObject(objectToAttach);
    }
}
