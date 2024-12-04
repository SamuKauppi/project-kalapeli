using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIColorAnimation : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private Color baseImageColor;
    [SerializeField] private Color warningColor = Color.red;

    [SerializeField] private Color baseTextColor;
    [SerializeField] private Color textWarningColor = Color.black;
    [SerializeField] private float transitionTime = 0.5f;
    [SerializeField] private bool looping = false;

    private void Start()
    {
        if (targetImage != null)
            targetImage.color = baseImageColor;

        if (targetText != null)
            targetText.color = baseTextColor;
    }

    public void SetImageActive(bool active)
    {
        if (!active)
            StopAllCoroutines();
        else
            StartEffect();

        if (targetImage != null)
            targetImage.gameObject.SetActive(active);
        if (targetText != null)
            targetText.gameObject.SetActive(active);
    }

    private void StartEffect()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateWarning());
    }

    private IEnumerator AnimateWarning()
    {
        Color currentColor = new();
        Color currentText = new();

        float time = 0f;

        while (time < transitionTime)
        {
            if (targetImage != null)
            {
                currentColor.r = Mathf.Lerp(warningColor.r, baseImageColor.r, time / transitionTime);
                currentColor.g = Mathf.Lerp(warningColor.g, baseImageColor.g, time / transitionTime);
                currentColor.b = Mathf.Lerp(warningColor.b, baseImageColor.b, time / transitionTime);
                currentColor.a = Mathf.Lerp(warningColor.a, baseImageColor.a, time / transitionTime);
                targetImage.color = currentColor;
            }

            if (targetText != null)
            {
                currentText.r = Mathf.Lerp(textWarningColor.r, baseTextColor.r, time / transitionTime);
                currentText.g = Mathf.Lerp(textWarningColor.g, baseTextColor.g, time / transitionTime);
                currentText.b = Mathf.Lerp(textWarningColor.b, baseTextColor.b, time / transitionTime);
                currentText.a = Mathf.Lerp(textWarningColor.a, baseTextColor.a, time / transitionTime);
                targetText.color = currentText;
            }

            time += Time.deltaTime;
            yield return null;
        }

        if (targetImage != null)
            targetImage.color = baseImageColor;

        if (targetText != null)
            targetText.color = baseTextColor;

        if (looping)
            StartEffect();
    }
}
