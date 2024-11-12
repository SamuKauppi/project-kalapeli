using System.Collections.Generic;
using UnityEngine;

public class MeshComparison : MonoBehaviour
{
    public static MeshComparison Instance { get; private set; }

    [SerializeField] private MeshWithData[] templateMeshes;

    private MeshProjection[] templateProjections;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        templateProjections = new MeshProjection[templateMeshes.Length];
        for (int i = 0; i < templateMeshes.Length; i++)
        {
            templateProjections[i] = new(templateMeshes[i].mesh);
        }
    }

    private float CompareProjections(List<Vector2> projection1, List<Vector2> projection2)
    {
        if (projection1.Count != projection2.Count)
        {
            Debug.LogError("Projections do not have the same number of vertices!");
            return 1f;
        }

        float totalDistance = 0f;
        for (int i = 0; i < projection1.Count; i++)
        {
            totalDistance += Vector2.Distance(projection1[i], projection2[i]);
        }

        return totalDistance / projection1.Count;
    }

    private float CompareMeshProjections(MeshProjection projection1, MeshProjection projection2, int targetCount)
    {
        projection1.ResampleProjection(targetCount);
        projection2.ResampleProjection(targetCount);

        float totalDistance = 0f;
        totalDistance += CompareProjections(projection1.ResampledXY, projection2.ResampledXY);
        totalDistance += CompareProjections(projection1.ResampledYZ, projection2.ResampledYZ);
        totalDistance += CompareProjections(projection1.ResampledXZ, projection2.ResampledXZ);

        if (totalDistance == float.NaN)
        {
            Debug.Log(projection1.ResampledXZ.Count);
            Debug.Log(targetCount);
        }

        return totalDistance;
    }

    public SwimmingType GetMatchingData(Mesh comparingMesh)
    {
        MeshProjection projectedMesh = new(comparingMesh);
        float minDistance = float.MaxValue;
        SwimmingType closestTemplate = SwimmingType.None;
        for (int i = 0; i < templateProjections.Length; i++)
        {
            int targetCount = templateProjections[i].TargetMesh.vertexCount <= comparingMesh.vertexCount ?
                templateProjections[i].TargetMesh.vertexCount : comparingMesh.vertexCount;

            float distance = CompareMeshProjections(projectedMesh, templateProjections[i], targetCount);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestTemplate = templateMeshes[i].swimmingType;
            }
        }

        return closestTemplate;
    }

}
