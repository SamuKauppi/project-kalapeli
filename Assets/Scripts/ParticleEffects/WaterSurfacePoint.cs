using UnityEngine;

public class WaterSurfacePoint : MonoBehaviour
{
    public static WaterSurfacePoint Instance { get; private set; }

    [SerializeField] private LayerMask mask;
    private Collider waterCollider;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        waterCollider = GetComponent<Collider>();
    }


    public Vector3 GetIntersectingPoint(Vector3 startPoint, Vector3 endPoint)
    {
        Vector3 direction = (endPoint - startPoint).normalized;
        if (Physics.Raycast(startPoint, direction, out RaycastHit hit, Vector3.Distance(startPoint, endPoint), mask))
        {
            if (hit.collider == waterCollider)
            {
                return hit.point;
            }
        }

        return Vector3.zero;
    }
}
