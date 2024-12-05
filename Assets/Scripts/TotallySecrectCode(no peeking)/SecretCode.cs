using System.Collections;
using UnityEngine;

public class SecretCode : MonoBehaviour
{
    [SerializeField] private GameObject Feesh;

    [SerializeField] private Transform[] pathPoints;
    private void Start()
    {
        Feesh.SetActive(false);
    }

    private IEnumerator MoveThroughPath()
    {
        Feesh.transform.position = pathPoints[0].position;
        for (int i = 0; i < 3; i++)
        {
            Transform t = pathPoints[i];
            LeanTween.cancel(Feesh);
            LeanTween.move(Feesh, t.position, 0.75f).setEase(LeanTweenType.easeOutQuint);
            LeanTween.rotate(Feesh, t.eulerAngles, 0.85f).setEase(LeanTweenType.easeOutElastic);
            yield return new WaitForSeconds(0.85f);
        }
        yield return new WaitForSeconds(0.5f);
        LeanTween.rotateAround(Feesh, Feesh.transform.forward, 1080f, 5f).setEase(LeanTweenType.easeOutBack);
        LeanTween.move(Feesh, pathPoints[3].position, 5f);
        yield return new WaitForSeconds(5f);
        Feesh.SetActive(false);
    }

    public void PlayAnimation()
    {
        Feesh.SetActive(true);
        LeanTween.cancel(Feesh);
        StopAllCoroutines();
        StartCoroutine(MoveThroughPath());
    }
}
