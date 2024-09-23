using System.Collections;
using UnityEngine;

public class DestroyPiece : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(KillMe());
    }

    IEnumerator KillMe()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}
