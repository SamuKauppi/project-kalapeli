using System.Collections.Generic;
using UnityEngine;

public class MeshProjectionVisualizer : MonoBehaviour
{
    public MeshProjection meshProjection;  // Reference to MeshProjection object
    public int targetVertexCount = 100;    // Number of points in the resampled contour
    public Color xyColor = Color.red;     // Color for XY projection
    public Color yzColor = Color.green;   // Color for YZ projection
    public Color xzColor = Color.blue;    // Color for XZ projection

    private void OnDrawGizmos()
    {
        if (meshProjection == null) return;

        meshProjection.ResampleProjection(targetVertexCount);

        // Draw each projection in a different color
        DrawProjection(meshProjection.ResampledXY, xyColor);
        DrawProjection(meshProjection.ResampledYZ, yzColor);
        DrawProjection(meshProjection.ResampledXZ, xzColor);
    }

    private void DrawProjection(List<Vector2> projection, Color color)
    {
        Gizmos.color = color;

        // Draw lines connecting each vertex in the projection
        for (int i = 0; i < projection.Count; i++)
        {
            Vector2 current = projection[i];
            Vector2 next = projection[(i + 1) % projection.Count];

            // Draw a line in the Scene view (z set to 0 for 2D projection)
            Gizmos.DrawLine(current, next);
        }
    }

    public void GiveMesh(MeshProjection meshProjection)
    {
        this.meshProjection = meshProjection;
    }
}