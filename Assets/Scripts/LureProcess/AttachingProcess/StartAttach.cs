using UnityEngine;

public class StartAttach : MonoBehaviour
{
    [SerializeField] private AttachingType objectToAttach;

    private void OnMouseDown()
    {
        AttachingProcess.Instance.StartAttachingObject(objectToAttach);
    }
}
