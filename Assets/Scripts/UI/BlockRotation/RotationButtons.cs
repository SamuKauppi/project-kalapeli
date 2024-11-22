using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class RotationButtons
{
    public Color normalColor = Color.white;
    public Color pressedColor = Color.white;
    public Button[] buttons;
    public float transitionDuration = 0.1f;

    private void SimulatePress(Button btn)
    {
        btn.StopAllCoroutines();
        btn.StartCoroutine(TransitionColor(btn.image, pressedColor, normalColor));
    }

    private IEnumerator TransitionColor(Image image, Color startColor, Color endColor)
    {
        image.color = startColor;
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            image.color = Color.Lerp(startColor, endColor, elapsedTime / transitionDuration);
            yield return null;
        }

        image.color = endColor;
    }

    public void ShowButtonClick(int sideDir, int upDir)
    {
        RotationDirection direction;

        if (sideDir == 0)
        {
            direction = upDir > 0 ? RotationDirection.Down : RotationDirection.Up;
        }
        else
        {
            direction = sideDir < 0 ? RotationDirection.Left : RotationDirection.Right;
        }

        Button targetButton = direction switch
        {
            RotationDirection.Up => buttons[0],
            RotationDirection.Down => buttons[1],
            RotationDirection.Left => buttons[2],
            RotationDirection.Right => buttons[3],
            _ => null
        };

        if (targetButton.gameObject.activeInHierarchy)
            SimulatePress(targetButton);
    }
}
