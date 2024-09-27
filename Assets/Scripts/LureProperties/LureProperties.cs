using UnityEngine;

public class LureProperties : MonoBehaviour
{
    // Stats
    public float Mass { get; private set; } = 0f;               // Is determined by volume of mesh
    public SwimmingType SwimmingType { get; private set; }      // Is determined by chub and how streamlined is the mesh
                                                                // Becomes bad if:
                                                                // - Has too many attachables
                                                                // - Has too many hooks
                                                                // - Has more than one chub
                                                                // - Is not streamlined enough 
    public float Depth { get; private set; } = 0f;              // Is determined by Weight and SwimmingType
    public Color[] Colors { get; private set; } = new Color[2]; // Is set by the player
    public AttachingType[] AttachedTypes { get; private set; }  // Is set by the player

    // private variables
    private MeshFilter _filter;
    private float streamLineRatio;

    private void Start()
    {
        _filter = GetComponent<MeshFilter>();
        UpdateStreamLineRatio();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log(streamLineRatio);
        }
    }


    float CalculateSurfaceArea(Vector3[] vertices, int[] triangles)
    {
        float area = 0f;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v1 = vertices[triangles[i]];
            Vector3 v2 = vertices[triangles[i + 1]];
            Vector3 v3 = vertices[triangles[i + 2]];

            area += CalculateTriangleArea(v1, v2, v3);
        }

        return area;
    }

    float CalculateTriangleArea(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        float a = Vector3.Distance(v1, v2);
        float b = Vector3.Distance(v2, v3);
        float c = Vector3.Distance(v3, v1);

        float s = (a + b + c) / 2; // Semi-perimeter
        return Mathf.Sqrt(s * (s - a) * (s - b) * (s - c)); // Heron's formula
    }

    float CalculateVolume(Vector3[] vertices, int[] triangles)
    {
        // Approximating volume for simple convex meshes
        float volume = 0f;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v1 = vertices[triangles[i]];
            Vector3 v2 = vertices[triangles[i + 1]];
            Vector3 v3 = vertices[triangles[i + 2]];

            volume += Vector3.Dot(v1, Vector3.Cross(v2, v3)) / 6f; // Tetrahedron volume formula
        }

        return Mathf.Abs(volume);
    }

    private float CalculateStreamliningRatio()
    {
        Vector3[] vertices = _filter.mesh.vertices;
        int[] triangles = _filter.mesh.triangles;
        float surfaceArea = CalculateSurfaceArea(vertices, triangles);
        float volume = CalculateVolume(vertices, triangles);
        float streamlineIndex = volume / surfaceArea;

        Vector3[] normals = _filter.mesh.normals; // Get normals for each vertex
        float alignmentFactor = 0f;

        // Calculate average alignment of normals with movement direction
        foreach (var normal in normals)
        {
            alignmentFactor += Vector3.Dot(normal.normalized, transform.forward.normalized);
        }

        alignmentFactor /= normals.Length; // Average alignment

        // Adjust streamline index based on alignment
        streamlineIndex *= (1 + alignmentFactor); // Increase index for better alignment

        return streamlineIndex; // Lower ratio indicates better streamlining
    }

    public void UpdateStreamLineRatio()
    {
        streamLineRatio = CalculateStreamliningRatio();
    }

    public void DetermineLureStats()
    {
        Mass = _filter.mesh.bounds.size.magnitude;
    }
    public int MatchingToFish()
    {

        return 0;
    }
}
