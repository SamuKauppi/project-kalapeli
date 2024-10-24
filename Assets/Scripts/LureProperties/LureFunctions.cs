using UnityEngine;

/// <summary>
/// Class used to determine stats for the lure while creating it
/// After the lure is finished and used, this script will be removed
/// </summary>
public class LureFunctions : MonoBehaviour
{
    public LureStats Stats { get; private set; }

    // SerializeFields
    [SerializeField] private float streamlineMultiplier = 1f;   // Multiplies streamlineRatio to make it more readable
    [SerializeField] private float uncutBlockLengthM = 0.1f;    // The desired length of uncut block in meters (used to convert from mesh to meters)
    [SerializeField] private float materialDensity = 500f;      // kg/m� (average for wood is 400 - 700 kg/m�)
    [SerializeField] private float thresholdStreamlineRatio;    // Min streamline ratio needed for a proper swimming type
    [SerializeField] private float minLengthOneHook = 1;        // Min length with one hook to maintain proper swimming
    [SerializeField] private float minLengthTwoHook = 2.5f;     // Min length with two hooks to maintain proper swimming
    [SerializeField] private float depthFromMassM = 0.1f;       // How much depth is added for each gram (default 10cm)

    // private variables
    private MeshFilter _filter;         // Reference to filter  
    private float unitConverter;        // The multiplier that converts uncut block length to desired length
    private float streamlineRatio;      // StreamlineRatio
    private float worstStreamlineRatio; // Starting streamline ratio
    private float attachWeight;         // Total weight of all attachments
    private float volume;               // volume of mesh
    private bool isDisplayingColor;     // Prevents multiple functions calls when no color should be shown
    private bool isStatBoundsSet;       // Detects if stat display bounds are set

    private void Start()
    {
        _filter = GetComponent<MeshFilter>();
        unitConverter = uncutBlockLengthM / _filter.mesh.bounds.size.x;
        Stats = GetComponent<LureStats>();
    }

    private void OnEnable()
    {
        AttachingProcess.OnAttach += CalculateStats;
        BlockPainter.OnColorChange += ChangeColors;
    }

    private void OnDisable()
    {
        AttachingProcess.OnAttach -= CalculateStats;
        BlockPainter.OnColorChange -= ChangeColors;
    }

    /// <summary>
    /// Calculates streamline ratio
    /// </summary>
    /// <returns>streamline ratio</returns>
    private float CalculateStreamlineRatio(Vector3[] vertices, int[] triangles, float volume)
    {
        // Calculate streamline index
        float surfaceArea = CalculateSurfaceArea(vertices, triangles);  // Calculate surface area
        // The higher the surface area is compared to volume, the less streamlined the mesh is
        float streamlineIndex = volume / surfaceArea;

        // Calculate how aligned to forward are normals
        float alignmentFactor = CalculateAlignment();

        // Adjust streamline index based on alignment
        streamlineIndex *= (1 + alignmentFactor);

        // Return index as ratio
        // Increase the value to make it more readable
        return streamlineIndex * streamlineMultiplier;
    }

    /// <summary>
    /// Calculates surface area of given vertices
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="triangles"></param>
    /// <returns></returns>
    private float CalculateSurfaceArea(Vector3[] vertices, int[] triangles)
    {
        float area = 0f;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            area += CalculateTriangleArea(
                vertices[triangles[i]],
                vertices[triangles[i + 1]],
                vertices[triangles[i + 2]]
            );
        }
        // Multiply with unit converter^2 since it's square meter
        return area * Mathf.Pow(unitConverter, 2);
    }

    /// <summary>
    /// Calculates the volume of mesh
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="triangles"></param>
    /// <returns></returns>
    private float CalculateVolume(Vector3[] vertices, int[] triangles)
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

        // Multiply with unitconverter^3 since it's cubic meters
        return Mathf.Abs(volume) * Mathf.Pow(unitConverter, 3);
    }

    /// <summary>
    /// Uses Heron's formula to calculate triangle area
    /// </summary>
    /// <param name="v1">vertex 1</param>
    /// <param name="v2">vertex 2</param>
    /// <param name="v3">vertex 3</param>
    /// <returns>Area of triangle</returns>
    private float CalculateTriangleArea(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        float a = Vector3.Distance(v1, v2);
        float b = Vector3.Distance(v2, v3);
        float c = Vector3.Distance(v3, v1);

        float s = (a + b + c) / 2; // Semi-perimeter
        return Mathf.Sqrt(s * (s - a) * (s - b) * (s - c)); // Heron's formula
    }

    /// <summary>
    /// Calculates the the normal alignment with transform.forward
    /// </summary>
    /// <returns></returns>
    private float CalculateAlignment()
    {
        Vector3[] normals = _filter.mesh.normals;
        float alignmentFactor = 0f;
        foreach (var normal in normals)
        {
            alignmentFactor += Vector3.Dot(normal.normalized, transform.forward.normalized);
        }

        alignmentFactor /= normals.Length; // Average alignment
        return alignmentFactor;
    }

    /// <summary>
    /// Returns group type from attaching type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private string GetAttachGroup(AttachingType type)
    {
        return type switch
        {
            AttachingType.Hook1 or AttachingType.Hook2 => "Hook",
            AttachingType.Eye1 or AttachingType.Eye2 or AttachingType.Eye3
            or AttachingType.Eye4 or AttachingType.Eye5 => "Eye",
            AttachingType.Chub1 or AttachingType.Chub2
            or AttachingType.Chub3 or AttachingType.Chub4 => "Chub",
            AttachingType.Fin1 or AttachingType.Fin2
            or AttachingType.Fin3 or AttachingType.Fin4 => "Fin",
            AttachingType.Tail1 or AttachingType.Tail2
            or AttachingType.Tail3 or AttachingType.Propeller => "Tail",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Calcuates the volume and streamlineratio
    /// </summary>
    /// <param name="type"></param>
    private void CalculateMeshStats(ref SwimmingType type)
    {
        // Get vertices and triangles
        Vector3[] vertices = _filter.mesh.vertices;
        int[] triangles = _filter.mesh.triangles;

        // Calculate volume
        volume = CalculateVolume(vertices, triangles);

        // Calculate streamlineratio and assgin volume
        streamlineRatio = CalculateStreamlineRatio(vertices, triangles, volume);

        // Set worstStreamlineRatio first time
        if (worstStreamlineRatio == 0)
        {
            worstStreamlineRatio = streamlineRatio;
        }

        // Lerp streamline ratio
        streamlineRatio = Mathf.InverseLerp(0, worstStreamlineRatio, streamlineRatio);


        if (streamlineRatio > thresholdStreamlineRatio)
        {
            type = SwimmingType.Bad;
        }
    }

    /// <summary>
    /// Calculates the stats from attached objects
    /// </summary>
    /// <param name="type"></param>
    /// <param name="depth"></param>
    private void CalculateAttachStats(ref SwimmingType type, ref float depth)
    {
        // Initialize local variabes
        bool stopChecking = false;
        float length = _filter.mesh.bounds.size.x * unitConverter;  // Convert units
        int hookCount = 0;
        int chubCount = 0;

        // Initialize class variables
        attachWeight = 0.0f;
        Stats.AttachedTypes = new AttachingType[transform.childCount - 1];

        for (int i = 1; i < transform.childCount; i++)
        {
            if (stopChecking) break;

            if (!transform.GetChild(i).TryGetComponent<AttachProperties>(out var attachProperties))
            {
                continue;
            }

            // Get the group
            string groupType = GetAttachGroup(attachProperties.AttachingType);

            // Add attachble weight
            attachWeight += attachProperties.Weight;
            Stats.AttachedTypes[i - 1] = attachProperties.AttachingType;

            // Switch statement to handle different attachable types
            switch (groupType)
            {
                case "Hook":
                    stopChecking = HandleHook(ref type, ref hookCount, length);
                    break;

                case "Chub":
                    stopChecking = HandleChub(ref type, ref depth, attachProperties as ChubProperties, ref chubCount);
                    break;

                default:
                    // Handle unknown group types if needed
                    break;
            }
        }
    }

    /// <summary>
    /// Handles special logic related to hooks
    /// </summary>
    /// <param name="type"></param>
    /// <param name="hookCount"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    private bool HandleHook(ref SwimmingType type, ref int hookCount, float length)
    {
        hookCount++;

        if ((length < minLengthOneHook && hookCount > 1) ||
            (length < minLengthTwoHook && hookCount > 2) ||
            hookCount > 3)
        {
            // Too many hooks
            type = SwimmingType.Bad;
            return true; // Indicate that checking should stop
        }

        return false; // Continue checking
    }

    /// <summary>
    /// Handles special lgic related to chubs
    /// </summary>
    /// <param name="type"></param>
    /// <param name="depth"></param>
    /// <param name="cp"></param>
    /// <param name="chubCount"></param>
    /// <returns></returns>
    private bool HandleChub(ref SwimmingType type, ref float depth, ChubProperties cp, ref int chubCount)
    {
        // Chub was not found
        if (!cp)
        {
            return true;
        }

        if (chubCount > 0)
        {
            // Too many chubs
            type = SwimmingType.Bad;
            return true; // Indicate that checking should stop
        }

        depth = cp.swimmingDepthInMeters;
        type = cp.swimmingType;
        chubCount++;

        return false; // Continue checking
    }

    /// <summary>
    /// Calculates the stats for lure
    /// </summary>
    public void CalculateStats()
    {
        // Initialize swimming style and depth
        SwimmingType type = SwimmingType.None;
        float depth = 0f;

        // Calculate stats related to mesh shape and set type bad if not streamlined enough
        CalculateMeshStats(ref type);

        if (type != SwimmingType.Bad && transform.childCount > 1)   // 1 child that already exists is arrow object and is ignored
        {
            // Calculate swimming type and depth based on attached objects
            CalculateAttachStats(ref type, ref depth);
        }

        // Calculate mass
        // (volume is calculated in CalculateMeshStats and attachWeight is calculated in CalculateAttachStats)
        Stats.Mass = volume * materialDensity;
        Stats.Mass *= 1000f;          // Convert to grams
        Stats.Mass += attachWeight;   // Add attachments weight (already in grams)

        // Increse swimming depth by X-m for each gram of weight
        depth += Stats.Mass * depthFromMassM;

        // Update variables
        Stats.SwimType = type;
        Stats.SwimmingDepth = depth;

        // Display stat changes
        if (isStatBoundsSet)
            StatDisplay.Instance.UpdateDisplayStats(Stats.SwimType,
                                                    streamlineRatio,
                                                    Stats.SwimmingDepth,
                                                    Stats.Mass,
                                                    Stats.BaseColor,
                                                    Stats.TexColor,
                                                    Stats.PatternID);
    }
    private void ChangeColors(Color baseC, Color texC, int textureID)
    {
        // Update variables
        Stats.BaseColor = baseC;
        Stats.TexColor = texC;
        Stats.PatternID = textureID;

        // Display stat changes
        if (isStatBoundsSet)
        {
            StatDisplay.Instance.UpdateDisplayStats(Stats.SwimType,
                                                    streamlineRatio,
                                                    Stats.SwimmingDepth,
                                                    Stats.Mass,
                                                    Stats.BaseColor,
                                                    Stats.TexColor,
                                                    Stats.PatternID);

            if (!isDisplayingColor)
            {
                StatDisplay.Instance.DisplayColors(true);
                isDisplayingColor = true;
            }
        }
    }

    public void ResetLure()
    {
        // Color is no longer displayed
        isDisplayingColor = false;
        StatDisplay.Instance.DisplayColors(false);
        Stats.BaseColor = Color.white;
        Stats.TexColor = Color.black;

        // Reset pattern
        Stats.PatternID = 1;

        // Destroy children skipping 1st (the arrow obj)
        for (int i = 1; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        // Recalculate stats to reset them
        CalculateStats();

        // If display bounds have not been made, make them
        if (!isStatBoundsSet)
        {
            StatDisplay.Instance.SetDisplayBounds(streamlineRatio, Stats.Mass);
            StatDisplay.Instance.UpdateDisplayStats(Stats.SwimType,
                                                    streamlineRatio,
                                                    Stats.SwimmingDepth,
                                                    Stats.Mass,
                                                    Stats.BaseColor,
                                                    Stats.TexColor,
                                                    Stats.PatternID);
            isStatBoundsSet = true;
        }
    }

    public void FinishLure()
    {
        if (transform.childCount == 0) return;

        Destroy(transform.GetChild(0).gameObject);  // Destroy first child (arrow)

        for (int i = 1; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).TryGetComponent<MoveAttach>(out var moveAttach))
            {
                moveAttach.enabled = false;
            }
        }
    }
}
