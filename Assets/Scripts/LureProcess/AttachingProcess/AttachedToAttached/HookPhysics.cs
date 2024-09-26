using UnityEngine;

public class HookPhysics : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 35.0f; // Deg/sec

    private bool shouldRotate;
    private Vector3 currentEuler;
    private Vector3 targetEuler;

    private void OnEnable()
    {
        BlockRotation.OnRotation += StartRotate;
    }
    private void OnDisable()
    {
        BlockRotation.OnRotation -= StartRotate;
    }

    private void Start()
    {
        targetEuler = transform.eulerAngles;
    }

    private void StartRotate(int sideId, int upId)
    {
        shouldRotate = true;
    }

    private void Update()
    {
        if (!shouldRotate) { return; }

        currentEuler = transform.localEulerAngles;

        if (currentEuler == targetEuler)
        {
            shouldRotate = false;
            return;
        }

        var step = rotationSpeed * Time.deltaTime;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(Vector3.down), step);
    }
}
