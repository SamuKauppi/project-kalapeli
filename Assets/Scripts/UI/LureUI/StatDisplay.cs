using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatDisplay : MonoBehaviour
{
    public static StatDisplay Instance { get; private set; }

    // Positioning
    [SerializeField] private GameObject displayWindow;
    [SerializeField] private Transform displayArrow;
    [SerializeField] private Transform onPos;
    [SerializeField] private Transform offPos;
    private bool isDiplaying = false;

    // Swimming style
    [SerializeField] private TMP_Text swimmingTypeText;
    [SerializeField] private Image badSwimImage;

    // Streamline
    [SerializeField] private Slider streamlineSlider;
    [SerializeField] private TMP_Text streamlineText;

    // Depth
    [SerializeField] private Slider depthSilder;
    [SerializeField] private TMP_Text depthText;
    [SerializeField] private float minDepth = 1.0f;             // Min depth (used only for display bounds)
    [SerializeField] private float maxDepth = 10f;              // Max depth (used only for display bounds)

    // Weight
    [SerializeField] private Slider weightSlider;
    [SerializeField] private TMP_Text weightText;

    // Color and texture
    [SerializeField] private Image baseColorImage;
    [SerializeField] private Image texColorImage;
    int lastID = -1;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        baseColorImage.gameObject.SetActive(false);
        texColorImage.gameObject.SetActive(false);
        ToggleStatDisplay(0f);
    }

    public void ToggleStatDisplay(float time)
    {
        isDiplaying = !isDiplaying;
        if (LeanTween.isTweening(displayWindow))
        {
            LeanTween.cancel(displayWindow);
        }
        Vector3 endPos = isDiplaying ? onPos.position : offPos.position;

        LeanTween.move(displayWindow, endPos, time).setEase(LeanTweenType.easeOutQuint);
        Vector3 scale = displayArrow.localScale;
        scale.x *= -1f;
        displayArrow.localScale = scale;
    }

    public void SetDisplayBounds(float maxStreamlineRatio, float maxWeight)
    {
        // Set depth bounds
        depthSilder.minValue = minDepth;
        depthSilder.maxValue = maxDepth;
        // Set streamline bounds 
        streamlineSlider.minValue = -maxStreamlineRatio;
        streamlineSlider.maxValue = 0f;
        // Set Weight bounds
        weightSlider.minValue = 0f;
        weightSlider.maxValue = maxWeight;
        // Hide bad swimming image
        badSwimImage.gameObject.SetActive(false);
    }

    public void UpdateDisplayStats(SwimmingType type,
                                   float streamlineRatio,
                                   float depth,
                                   float weight,
                                   Color baseC,
                                   Color texC,
                                   int textureId)
    {
        swimmingTypeText.text = type.ToString();
        if (type == SwimmingType.Bad)
        {
            badSwimImage.gameObject.SetActive(true);
        }
        else
        {
            badSwimImage.gameObject.SetActive(false);
        }
        streamlineText.text = MathF.Round(streamlineRatio, 2).ToString();
        streamlineSlider.value = -streamlineRatio;
        depthText.text = MathF.Round(depth, 2).ToString() + "m";
        depthSilder.value = depth;
        weightText.text = MathF.Round(weight, 2).ToString() + "g";
        weightSlider.value = weight;
        baseColorImage.color = baseC;
        texColorImage.color = texC;
        if (textureId != lastID)
        {
            texColorImage.sprite = BlockPainter.Instance.GetTextureSprites(textureId);
        }
        lastID = textureId;
    }

    public void DisplayColors(bool value)
    {
        baseColorImage.gameObject.SetActive(value);
        texColorImage.gameObject.SetActive(value);
    }
}
