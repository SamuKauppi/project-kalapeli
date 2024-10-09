using System.Collections;
using UnityEngine;

/// <summary>
/// Rotates the object it is attached to
/// </summary>
public class BlockRotation : MonoBehaviour
{
    public static BlockRotation Instance { get; private set; }

    // Is block rotating (disable cutting and attaching while true)
    public bool IsRotating { get; private set; }

    // Stops the ability to rotate
    public bool StopRotating { get; set; } = false;

    public delegate void Rotate(int sideRot, int upRot);
    public static event Rotate OnRotationStart;
    public static event Rotate OnRotationEnd;

    // Rotations
    [SerializeField] private float rotationTime = 0.5f; // How fast the block rotates

    public Quaternion DefaultRotation { get { return defaultRot; } }

    private Quaternion defaultRot;  // Starting rotation
    private int sideRotIndex = 0;   // Which side rotation is used
    private int upRotIndex = 0;     // Which up rotation is used

    // Used to avoid restart same up rotation corountine
    private UpRotation currentRotDir = UpRotation.None;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        defaultRot = transform.rotation;
    }

    private void Update()
    {
        if (StopRotating) { return; }

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) &&
            currentRotDir != UpRotation.Up)
        {
            if (IsRotating) StopAllCoroutines();
            if (upRotIndex == 0)
                currentRotDir = UpRotation.Up;
            StartCoroutine(RotateTransform(0, -1, rotationTime));
        }

        if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) &&
            currentRotDir != UpRotation.Down)
        {
            if (IsRotating) StopAllCoroutines();
            if (upRotIndex == 0)
                currentRotDir = UpRotation.Down;
            StartCoroutine(RotateTransform(0, 1, rotationTime));
        }

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (IsRotating) StopAllCoroutines();
            StartCoroutine(RotateTransform(-1 * (upRotIndex == 0 ? 1 : upRotIndex), 0, rotationTime));
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (IsRotating) StopAllCoroutines();
            StartCoroutine(RotateTransform(1 * (upRotIndex == 0 ? 1 : upRotIndex), 0, rotationTime));
        }

    }

    /// <summary>
    /// Rotate object based on index
    /// </summary>
    /// <param name="sideRotDir">Direction it rotates up axis</param>
    /// <param name="upRotDir">Direction it rotates side axis</param>
    /// <param name="rotTime">How long it takes to rotate</param>
    /// <returns></returns>
    private IEnumerator RotateTransform(int sideRotDir, int upRotDir, float rotTime)
    {
        IsRotating = true;
        // Initialize rotations
        Quaternion currentRot = transform.rotation;
        Quaternion targetRot = defaultRot;

        // Get next index
        sideRotIndex = (sideRotIndex + sideRotDir + 4) % 4;
        upRotIndex = Mathf.Clamp(upRotIndex + upRotDir, -1 , 1);

        targetRot *= Quaternion.Euler(90f * upRotIndex * Vector3.right);
        targetRot *= Quaternion.Euler(0f, 90f * sideRotIndex, 0f);

        OnRotationStart?.Invoke(sideRotIndex, upRotIndex);
        if (currentRot != targetRot)
        {
            float time = 0.0f;

            while (time < rotTime)
            {
                time += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, time / rotTime);
                transform.rotation = Quaternion.Lerp(currentRot, targetRot, t);
                yield return null;
            }
            transform.rotation = targetRot;
        }

        IsRotating = false;
        OnRotationEnd?.Invoke(sideRotIndex, upRotIndex);
        currentRotDir = upRotIndex < 0 ? UpRotation.Up : upRotIndex > 0 ? UpRotation.Down : UpRotation.None;
    }

    /// <summary>
    /// Reset rotation
    /// </summary>
    public void ResetRotation()
    {
        sideRotIndex = 0;
        upRotIndex = 0;
        StartCoroutine(RotateTransform(0, 0, 0.1f));
    }
}

public enum UpRotation
{
    None,
    Up,
    Down
}
