using System.Collections;
using UnityEngine;

public class DestroyPiece : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(KillMe());
    }

    private void OnDisable()
    {
        Destroy(gameObject);
    }

    private IEnumerator KillMe()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}
