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
    [SerializeField] private RotationButtons rotationButtons;
    private int sideRotIndex = 0;   // Which side rotation is used
    private int upRotIndex = 0;     // Which up rotation is used


    // Used to avoid restart same up-rotation corountine and simlating button press
    private RotationDirection currentRotDir = RotationDirection.Middle;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        if (StopRotating) { return; }

        RotateByKeys();
    }

    private void RotateByKeys()
    {
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) &&
            currentRotDir != RotationDirection.Up)
        {
            if (IsRotating) StopAllCoroutines();
            if (upRotIndex == 0)
                currentRotDir = RotationDirection.Up;
            StartCoroutine(RotateTransform(0, -1, rotationTime));
            rotationButtons.ShowButtonClick(0, -1);
        }

        if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) &&
            currentRotDir != RotationDirection.Down)
        {
            if (IsRotating) StopAllCoroutines();
            if (upRotIndex == 0)
                currentRotDir = RotationDirection.Down;
            StartCoroutine(RotateTransform(0, 1, rotationTime));
            rotationButtons.ShowButtonClick(0, 1);
        }

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (IsRotating) StopAllCoroutines();
            StartCoroutine(RotateTransform(1 * (upRotIndex == 0 ? 1 : upRotIndex), 0, rotationTime));
            rotationButtons.ShowButtonClick(1, 0);
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (IsRotating) StopAllCoroutines();
            StartCoroutine(RotateTransform(-1 * (upRotIndex == 0 ? 1 : upRotIndex), 0, rotationTime));
            rotationButtons.ShowButtonClick(-1, 0);
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
        GetTargetRotation(sideRotDir, upRotDir, out Quaternion currentRot, out Quaternion targetRot);
        if (rotTime > 0)
        {
            SoundManager.Instance.PlaySound(SoundClipTrigger.OnLureTurn);
        }

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
            targetRot = GetCurrentTargetRot();
        }
        transform.rotation = targetRot;
        IsRotating = false;
        OnRotationEnd?.Invoke(sideRotIndex, upRotIndex);
        // This sets rotation for up or down to max preventing jittering
        currentRotDir = upRotIndex < 0 ? RotationDirection.Up : upRotIndex > 0 ? RotationDirection.Down : RotationDirection.Middle;
    }

    private void GetTargetRotation(int sideRotDir, int upRotDir, out Quaternion currentRot, out Quaternion targetRot)
    {
        // Initialize rotations
        currentRot = transform.rotation;

        // Get next index
        sideRotIndex = (sideRotIndex + sideRotDir + 4) % 4;
        upRotIndex = Mathf.Clamp(upRotIndex + upRotDir, -1, 1);

        // Get current rotation
        targetRot = GetCurrentTargetRot();
    }

    private Quaternion GetCurrentTargetRot()
    {
        return GameManager.Instance.MainCamera.transform.rotation *
            Quaternion.Euler(90f * upRotIndex * Vector3.right) *
            Quaternion.Euler(90f * sideRotIndex * Vector3.up);
    }

    /// <summary>
    /// Reset rotation
    /// </summary>
    public void ResetRotation(float timeToReset)
    {
        sideRotIndex = 0;
        upRotIndex = 0;
        StartCoroutine(RotateTransform(0, 0, timeToReset));
    }

    public void RotateBlockSide(int dir)
    {
        if (IsRotating) StopAllCoroutines();
        StartCoroutine(RotateTransform(dir * (upRotIndex == 0 ? 1 : upRotIndex), 0, rotationTime));
    }

    public void RotateBlockUp(int dir)
    {

        if ((dir == 1 && currentRotDir == RotationDirection.Down) ||
            (dir == -1 && currentRotDir == RotationDirection.Up))
        {
            return;
        }

        if (IsRotating) StopAllCoroutines();

        if (upRotIndex == 0)
        {
            currentRotDir = dir > 0 ? RotationDirection.Down : RotationDirection.Up;
        }

        StartCoroutine(RotateTransform(0, dir, rotationTime));
    }
}
