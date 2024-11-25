using System.Collections;
using UnityEngine;

public class CuttingAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform cuttingTransform;
    [SerializeField] private RectTransform cutPosition1;
    [SerializeField] private RectTransform cutPosition2;
    [SerializeField] private float animationTime;
    [SerializeField] private float fadeTime;
    [SerializeField] private int repeats;

    private int animationPositionCounter = 0;
    private int repeatCounter = 0;
    private float startHeight = 0f;

    private void Start()
    {
        cuttingTransform.gameObject.SetActive(false);
        startHeight = cuttingTransform.rect.height;
    }

    private IEnumerator ScaleTransform(RectTransform targetTransform, Vector3 targetPos)
    {
        yield return new WaitForSeconds(0.5f);

        float distance = Vector3.Distance(targetTransform.position, targetPos);
        float currentHeight;
        float time = 0f;
        while (time < animationTime)
        {
            time += Time.deltaTime;
            currentHeight = Mathf.Lerp(startHeight, distance, time / animationTime);
            targetTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, currentHeight);
            yield return null;
        }

        yield return new WaitForSeconds(fadeTime);

        if (repeatCounter < repeats) 
        { 
            repeatCounter++;
            targetTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, startHeight);
            
            animationPositionCounter++;
            Vector3 newTarget = animationPositionCounter % 2 == 0 ? cutPosition1.position : cutPosition2.position;
            cuttingTransform.up = newTarget - cuttingTransform.position;
            StartCoroutine(ScaleTransform(targetTransform, newTarget));
        }
        else
        {
            targetTransform.gameObject.SetActive(false);
            Tutorial.Instance.EndCuttingTutorial();
        }
    }

    public void ScaleTowards()
    {
        cuttingTransform.gameObject.SetActive(true);
        cuttingTransform.up = cutPosition1.position - cuttingTransform.position;

        StartCoroutine(ScaleTransform(cuttingTransform, cutPosition1.position));
    }

    public void EndAnimation()
    {
        StopAllCoroutines();
        cuttingTransform.gameObject.SetActive(false);
    }
}
