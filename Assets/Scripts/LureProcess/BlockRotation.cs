using System.Collections;
using UnityEngine;


/// <summary>
/// Rotates the object it is attached to
/// </summary>
public class BlockRotation : MonoBehaviour
{

    // Is block rotating (disable cutting and attaching while true)
    public bool IsRotating { get; private set; }

    // Stops the ability to rotate
    public bool StopRotating { get; set; } = false;

    // Rotations
    [SerializeField] private float cameraRotationTime = 0.5f;                   // How fast the block rotates
    private Quaternion sideRot;
    private Quaternion frontRot;
    private Quaternion backRot;
    private Quaternion otherSideRot;

    private int rotIndex = 0;   // Which rotation angle is being used now

    private void Start()
    {
        sideRot = transform.rotation;
        frontRot = transform.rotation * Quaternion.Euler(0f, 0f, 90f);
        backRot = transform.rotation * Quaternion.Euler(0f, 0f, -90f);
        otherSideRot = transform.rotation * Quaternion.Euler(0f, 0f, 180f);
    }
    private void Update()
    {
        if (StopRotating) { return; }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (IsRotating) StopAllCoroutines();
            StartCoroutine(RotateCamera(1, 0));
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (IsRotating) StopAllCoroutines();
            StartCoroutine(RotateCamera(-1, 0));
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (IsRotating) StopAllCoroutines();
            StartCoroutine(RotateCamera(0, -1));
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (IsRotating) StopAllCoroutines();
            StartCoroutine(RotateCamera(0, 1));
        }
    }

    private IEnumerator RotateCamera(int upRotDir, int sideRotDir)
    {
        IsRotating = true;
        Quaternion currentRot = transform.rotation;
        Quaternion targetRot;

        if (upRotDir != 0)
        {
            targetRot = upRotDir < 0 ? GetSideRotation(sideRotDir) : Quaternion.Euler(-90f, 0, 0) * GetSideRotation(sideRotDir);
        }
        else
        {
            targetRot = GetSideRotation(sideRotDir);
        }

        if (currentRot != targetRot)
        {
            float time = 0.0f;

            while (time < cameraRotationTime)
            {
                time += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, time / cameraRotationTime);
                transform.rotation = Quaternion.Lerp(currentRot, targetRot, t);
                yield return null;
            }
            transform.rotation = targetRot;
        }

        IsRotating = false;
    }


    private Quaternion GetSideRotation(int xRot)
    {
        rotIndex = (rotIndex + xRot + 4) % 4;
        return rotIndex switch
        {
            0 => sideRot,
            1 => frontRot,
            2 => otherSideRot,
            3 => backRot,
            _ => sideRot,
        };
    }
}
