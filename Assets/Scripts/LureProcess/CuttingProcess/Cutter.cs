using EzySlice;
using UnityEngine;

public class Cutter : MonoBehaviour
{
    public static Cutter Instance { get; private set; }

    [SerializeField] private GameObject cutPiecePrefab;
    [SerializeField] private Material blockMaterial;
    [SerializeField] private float physicsStrength;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private float GetMeshOffset(Mesh targetMesh)
    {
        Vector3[] vertices = targetMesh.vertices;
        Vector3 avgPosition = Vector3.zero;

        for (int i = 0; i < vertices.Length; i++)
        {
            avgPosition += vertices[i];
        }

        avgPosition /= vertices.Length;

        return Vector3.Distance(avgPosition, Vector3.zero);
    }

    public void PerformCut(GameObject objectToCut, Vector3 cutPosition, Vector3 planeNormal)
    {
        if (objectToCut == null) return;

        // Perform slice
        SlicedHull hull = objectToCut.Slice(cutPosition, planeNormal, blockMaterial);

        if (hull == null) return;

        // Getting distances to center
        float distanceToLower = GetMeshOffset(hull.lowerHull);
        float distanceToUpper = GetMeshOffset(hull.upperHull);

        // Getting meshes
        Mesh closerMesh = distanceToUpper < distanceToLower ? hull.upperHull : hull.lowerHull;
        Mesh furtherMesh = distanceToUpper >= distanceToLower ? hull.upperHull : hull.lowerHull;

        // Main mesh
        RecalculateUvs(closerMesh);
        objectToCut.GetComponent<MeshFilter>().mesh = closerMesh;
        objectToCut.GetComponent<MeshCollider>().sharedMesh = closerMesh;

        // Cut piece
        GameObject cutPiece = Instantiate(cutPiecePrefab, objectToCut.transform.position, objectToCut.transform.rotation);
        cutPiece.GetComponent<MeshFilter>().mesh = furtherMesh;
        cutPiece.GetComponent<MeshRenderer>().material = blockMaterial;
        cutPiece.GetComponent<MeshCollider>().sharedMesh = furtherMesh;

        // Ensure planeNormal pushes the cut piece outward
        Vector3 cutDirection = (objectToCut.transform.position - cutPosition).normalized;
        if (Vector3.Dot(planeNormal, cutDirection) < 0)
        {
            planeNormal = -planeNormal;
        }

        float angle = Mathf.Abs(Vector3.Angle(cutDirection, Vector3.up));
        float forceMultiplier = Mathf.InverseLerp(0f, 180f, angle);
        float force = physicsStrength * forceMultiplier;
        cutPiece.GetComponent<Rigidbody>().AddForce(-planeNormal.normalized * force);
    }

    public void RecalculateUvs(Mesh mesh)
    {
        // Get vertices and uvs
        Vector3[] _vertices = mesh.vertices;
        Vector2[] _uvs = new Vector2[_vertices.Length];

        // Get the bounds
        float minX = float.MaxValue, minY = float.MaxValue; // Min x & y
        float maxX = float.MinValue, maxY = float.MinValue; // Max x & y

        for (int i = 0; i < _vertices.Length; i++)
        {
            minX = Mathf.Min(minX, _vertices[i].x);
            minY = Mathf.Min(minY, _vertices[i].y);
            maxX = Mathf.Max(maxX, _vertices[i].x);
            maxY = Mathf.Max(maxY, _vertices[i].y);
        }

        // Generate uvs
        for (int i = 0; i < _vertices.Length; i++)
        {
            Vector3 vertex = _vertices[i];

            _uvs[i] = new Vector2(1f - Mathf.InverseLerp(minX, maxX, vertex.x),
                                  Mathf.InverseLerp(minY, maxY, vertex.y));

            // Clamp the values to avoid texture bug
            _uvs[i].x = Mathf.Clamp(_uvs[i].x, 0.001f, 0.999f);
            _uvs[i].y = Mathf.Clamp(_uvs[i].y, 0.001f, 0.999f);
        }
        mesh.uv = _uvs;
    }
}
