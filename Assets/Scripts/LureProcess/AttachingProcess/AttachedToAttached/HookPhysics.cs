using UnityEngine;

public class HookPhysics : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5.0f;
    private Transform parent;
    private bool isParentRotating;
    private bool shouldRotate;

    Quaternion targetRotation;
    Quaternion worldDownAlignment;

    private void OnEnable()
    {
        BlockRotation.OnRotationStart += StartRotate;
        BlockRotation.OnRotationEnd += StopRotate;
    }
    private void OnDisable()
    {
        BlockRotation.OnRotationStart -= StartRotate;
        BlockRotation.OnRotationEnd -= StopRotate;
    }

    private void StartRotate(int sideId, int upId)
    {
        isParentRotating = true;
        shouldRotate = true;
    }

    private void StopRotate(int sideId, int upId)
    {
        isParentRotating = false;
    }

    private void Update()
    {
        if (!shouldRotate) { return; }

        if (parent)
        {
            targetRotation = parent.rotation;
            worldDownAlignment = Quaternion.FromToRotation(transform.up, Vector3.up) * targetRotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, worldDownAlignment, Time.deltaTime * rotationSpeed);

            if (!isParentRotating && Quaternion.Angle(transform.rotation, worldDownAlignment) < 0.01f)
            {
                shouldRotate = false;
            }
        }
        else
        {
            parent = transform.parent;
        }
    }
}
