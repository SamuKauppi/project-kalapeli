using UnityEngine;
using UnityEngine.UI;
public class ColorWheel : MonoBehaviour
{
    [SerializeField] private bool randomColorAtStart = true;
    [SerializeField] private Image colorImage;          // Gradient image that user uses to pick color

    [SerializeField] private Image colorPickSprite;     // Small circle that user uses to pick color
    [SerializeField] private Slider hueSlider;

    // Preview
    [SerializeField] private Image previewImage;            // Previews the color in a image
    [SerializeField] private MeshRenderer previewRenderer;  // Previews the color in a renderer
    private string colorType;
    private Material[] mats;

    // Final
    private Image targetImage;                              // Image displayed when the color picker is not active

    private Color newColor;
    private float hue = 0;
    private float saturation = 0;
    private float brightness = 0;
    private bool colorIsBeingHeld;

    private bool IsActive { get; set; }

    private void Start()
    {
        if (randomColorAtStart)
        {
            newColor = new(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
            SetColor();
        }
    }
    private void Update()
    {
        if (!IsActive)
            return;

        if (Input.GetMouseButtonDown(0) && RectTransformUtility.RectangleContainsScreenPoint(colorImage.rectTransform, Input.mousePosition))
        {
            colorIsBeingHeld = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            colorIsBeingHeld = false;
        }

        if (colorIsBeingHeld)
        {

            // Get the normalized position of the mouse within the color image
            RectTransformUtility.ScreenPointToLocalPointInRectangle(colorImage.rectTransform, Input.mousePosition, null, out Vector2 localMousePos);

            // Clamp the sprite's position to stay within the color image
            float clampedX = Mathf.Clamp(localMousePos.x, -colorImage.rectTransform.rect.width / 2f, colorImage.rectTransform.rect.width / 2f);
            float clampedY = Mathf.Clamp(localMousePos.y, -colorImage.rectTransform.rect.height / 2f, colorImage.rectTransform.rect.height / 2f);
            colorPickSprite.rectTransform.anchoredPosition = new Vector2(clampedX, clampedY);


            // Calculate the normalized position within the color image
            saturation = Mathf.Clamp01((localMousePos.x + colorImage.rectTransform.rect.width / 2f) / colorImage.rectTransform.rect.width);
            brightness = Mathf.Clamp01((localMousePos.y + colorImage.rectTransform.rect.height / 2f) / colorImage.rectTransform.rect.height);

            // Convert HSV to RGB color
            newColor = Color.HSVToRGB(hue, saturation, brightness);
        }

        if (newColor != previewImage.color)
        {
            ShowSelectedColor();
        }
    }

    private void UpdateSelectedColorUI(Color c)
    {
        // Extract hsv values
        Color.RGBToHSV(c, out float newHue, out  float newSaturation, out float newBrightness);

        // Update variables
        hue = newHue;
        saturation = newSaturation;
        brightness = newBrightness;
        newColor = c;

        // Update slider
        hueSlider.value = newHue;

        // Update pick sprite position
        float width = colorImage.rectTransform.rect.width * 0.5f;
        float height = colorImage.rectTransform.rect.height * 0.5f;

        float xPos = Mathf.Lerp(-width, width, newSaturation);
        float yPos = Mathf.Lerp(-height, height, newBrightness);

        colorPickSprite.rectTransform.anchoredPosition = new(xPos, yPos);

        ShowSelectedColor();
    }

    private void ShowSelectedColor()
    {
        if (previewImage)
            previewImage.color = newColor;

        if (previewRenderer)
        {
            // Iterate through the materials and modify them
            for (int i = 0; i < previewRenderer.materials.Length; i++)
            {
                mats[i].SetColor(colorType, newColor);
            }

            previewRenderer.materials = mats;
        }
    }

    public void ActivateColorPicker(Image targetImage, Color targetColor, string type)
    {
        mats = previewRenderer.materials;
        this.targetImage = targetImage;
        colorType = type;
        IsActive = true;
        UpdateSelectedColorUI(targetColor);
    }
    public void ChangeHue(float sliderValue)
    {
        hue = sliderValue;
        newColor = Color.HSVToRGB(hue, saturation, brightness);
        colorImage.color = Color.HSVToRGB(hue, 1f, 1f);
    }

    public void SetColor()
    {
        IsActive = false;
        targetImage.color = newColor;
    }

    public void CancelColor()
    {
        IsActive = false;
        targetImage = null;
    }
}
