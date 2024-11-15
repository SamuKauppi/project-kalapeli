using UnityEngine;

public class TestMoving : MonoBehaviour
{
    [SerializeField] private float maxMove = 15f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float rotSpeed = 1f;

    private float dir = 1f;

    private void FixedUpdate()
    {
        transform.position += speed * dir * Time.deltaTime * Vector3.right;
        transform.Rotate(Vector3.up, rotSpeed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x) > maxMove)
        {
            dir *= -1f;
        }
    }
}
