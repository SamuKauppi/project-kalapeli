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
    public static event Rotate OnRotation;

    // Rotations
    [SerializeField] private float cameraRotationTime = 0.5f; // How fast the block rotates
    private Quaternion sideRot;
    private Quaternion frontRot;
    private Quaternion backRot;
    private Quaternion otherSideRot;

    private int sideRotIndex = 0;   // Which side rotation angle is being used now
    private int upRotIndex = -1;    // Which up rotation is used (< 0 = side and > 0 = up)

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        sideRot = transform.rotation;
        frontRot = transform.rotation * Quaternion.Euler(0f, 90f, 0);
        backRot = transform.rotation * Quaternion.Euler(0f, -90f, 0f);
        otherSideRot = transform.rotation * Quaternion.Euler(0f, 180f, 0f);
    }
    private void Update()
    {
        if (StopRotating) { return; }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (IsRotating) StopAllCoroutines();
            StartCoroutine(RotateTransform(1, 0, cameraRotationTime));
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (IsRotating) StopAllCoroutines();
            StartCoroutine(RotateTransform(-1, 0, cameraRotationTime));
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (IsRotating) StopAllCoroutines();
            StartCoroutine(RotateTransform(upRotIndex, -1, cameraRotationTime));
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (IsRotating) StopAllCoroutines();
            StartCoroutine(RotateTransform(upRotIndex, 1, cameraRotationTime));
        }
    }

    /// <summary>
    /// Rotate object based on index
    /// </summary>
    /// <param name="upRotDir">Direction it rotates so that it shows top</param>
    /// <param name="sideRotDir">Direction it rotates side axis</param>
    /// <param name="rotTime">How long it takes to rotate</param>
    /// <returns></returns>
    private IEnumerator RotateTransform(int upRotDir, int sideRotDir, float rotTime)
    {
        IsRotating = true;
        Quaternion currentRot = transform.rotation;
        Quaternion targetRot = transform.rotation;

        if (upRotDir != 0)
        {
            upRotIndex = upRotDir;
            targetRot = upRotDir < 0 ? GetSideRotation(sideRotDir) : Quaternion.Euler(-90f, 0, 0) * GetSideRotation(sideRotDir);
        }
        else if (sideRotDir != 0)
        {
            targetRot = GetSideRotation(sideRotDir);
        }

        OnRotation?.Invoke(sideRotIndex, upRotIndex);
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
    }

    /// <summary>
    /// Get side rotation
    /// </summary>
    /// <param name="rotDir">index increase</param>
    /// <returns></returns>
    private Quaternion GetSideRotation(int rotDir)
    {
        sideRotIndex = (sideRotIndex + rotDir + 4) % 4;
        return sideRotIndex switch
        {
            0 => sideRot,
            1 => frontRot,
            2 => otherSideRot,
            3 => backRot,
            _ => sideRot,
        };
    }

    /// <summary>
    /// Reset rotation
    /// </summary>
    public void ResetRotation()
    {
        sideRotIndex = 0;
        StartCoroutine(RotateTransform(-1, 0, 0.1f));
    }
}
