using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshProjection
{
    public Mesh TargetMesh { get { return targetMesh; } }
    public List<Vector2> ResampledXY { get; private set; }
    public List<Vector2> ResampledYZ { get; private set; }
    public List<Vector2> ResampledXZ { get; private set; }

    private readonly Mesh targetMesh;
    private readonly List<Vector2> XY = new();
    private readonly List<Vector2> YZ = new();
    private readonly List<Vector2> XZ = new();

    public MeshProjection(Mesh target)
    {
        targetMesh = target;

        // Store the projection values
        ProjectVerticesToPlane(target, "XY");
        XY = ComputeConvexHull(XY);
        ProjectVerticesToPlane(target, "YZ");
        YZ = ComputeConvexHull(YZ);
        ProjectVerticesToPlane(target, "XZ");
        XZ = ComputeConvexHull(XZ);
    }

    public void ResampleProjection(int targetCount)
    {
        // Resample data with target count
        ResampledXY = OrderAndAlignContour(ResampleContour(XY, targetCount));
        ResampledXZ = OrderAndAlignContour(ResampleContour(XZ, targetCount));
        ResampledYZ = OrderAndAlignContour(ResampleContour(YZ, targetCount));
    }

    private void ProjectVerticesToPlane(Mesh mesh, string plane)
    {
        Vector3[] vertices = mesh.vertices;

        // Projects vertices to a plane
        foreach (var vertex in vertices)
        {
            if (plane == "XY") XY.Add(vertex);
            else if (plane == "YZ") YZ.Add(new Vector2(vertex.y, vertex.z));
            else if (plane == "XZ") XZ.Add(new Vector2(vertex.x, vertex.z));
        }
    }

    List<Vector2> ComputeConvexHull(List<Vector2> points)
    {
        if (points.Count < 3)
            return points;

        // Find the point with the lowest y-coordinate, break ties by x-coordinate
        points.Sort((a, b) => a.y == b.y ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
        Vector2 pivot = points[0];

        // Sort points by polar angle with the pivot
        points = points.OrderBy(p => Mathf.Atan2(p.y - pivot.y, p.x - pivot.x)).ToList();

        // Use a stack to build the convex hull
        Stack<Vector2> hull = new();
        hull.Push(points[0]);
        hull.Push(points[1]);

        for (int i = 2; i < points.Count; i++)
        {
            Vector2 top = hull.Pop();
            while (hull.Count > 0 && CrossProduct(hull.Peek(), top, points[i]) <= 0)
            {
                top = hull.Pop();
            }
            hull.Push(top);
            hull.Push(points[i]);
        }

        return hull.ToList();
    }

    float CrossProduct(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
    }


    List<Vector2> ResampleContour(List<Vector2> contour, int targetCount)
    {
        // Step 1: Calculate cumulative distances along the contour
        List<float> cumulativeDistances = new() { 0f };
        for (int i = 1; i < contour.Count; i++)
        {
            // Distance between consecutive points
            float distance = Vector2.Distance(contour[i - 1], contour[i]);
            // Add to cumulative distance from the starting point
            cumulativeDistances.Add(cumulativeDistances[i - 1] + distance);
        }

        // Total perimeter of the contour
        float totalPerimeter = cumulativeDistances[^1];
        // Interval distance between new points
        float interval = totalPerimeter / targetCount;

        // Step 2: Resample points along the contour at equal intervals
        List<Vector2> resampled = new();
        int j = 0; // index for the original contour points

        for (int i = 0; i < targetCount; i++)
        {
            // Distance along the perimeter where the new point should be placed
            float targetDistance = i * interval;

            // Find the original contour segment that contains targetDistance
            while (cumulativeDistances[j + 1] < targetDistance) j++;

            // Calculate interpolation ratio along this segment
            float ratio = (targetDistance - cumulativeDistances[j]) /
                          (cumulativeDistances[j + 1] - cumulativeDistances[j]);

            // Interpolate to find the exact position of the new point
            Vector2 point = Vector2.Lerp(contour[j], contour[j + 1], ratio);
            resampled.Add(point);
        }

        return resampled;
    }

    List<Vector2> OrderAndAlignContour(List<Vector2> contour)
    {
        // Find the starting vertex, e.g., the one with the lowest x-value
        int startIndex = 0;
        for (int i = 1; i < contour.Count; i++)
        {
            if (contour[i].x < contour[startIndex].x ||
                (contour[i].x == contour[startIndex].x && contour[i].y < contour[startIndex].y))
            {
                startIndex = i;
            }
        }

        // Reorder so the contour starts from this index
        List<Vector2> orderedContour = new();
        orderedContour.AddRange(contour.Skip(startIndex));
        orderedContour.AddRange(contour.Take(startIndex));

        // Ensure the winding order is consistent (e.g., clockwise)
        if (!IsClockwise(orderedContour))
        {
            orderedContour.Reverse();
        }

        return orderedContour;
    }

    bool IsClockwise(List<Vector2> contour)
    {
        float sum = 0f;
        for (int i = 0; i < contour.Count; i++)
        {
            Vector2 current = contour[i];
            Vector2 next = contour[(i + 1) % contour.Count];
            sum += (next.x - current.x) * (next.y + current.y);
        }
        return sum > 0;
    }
}
