using UnityEngine;
using UnityEngine.UI;

public class BlockPainter : MonoBehaviour
{
    public static BlockPainter Instance { get; private set; }

    // Object references
    [SerializeField] private ScrollTextureContainer scrollContainer;
    [SerializeField] private ColorWheel colorWheel;
    [SerializeField] private MeshRenderer blockRenderer;
    [SerializeField] private Image baseColorButton;
    [SerializeField] private Image textureColorButton;

    // Matrial properties
    [SerializeField] private Texture2D[] lureTextures;
    [SerializeField] private Material coloredBlockMaterial;

    // Coloring variables
    private Color baseColor;
    private Color textureColor;
    private int selectedTextureId = 1;

    // const
    private const string BASE_COLOR = "_BaseColor";
    private const string TEX_COLOR = "_TextureColor";

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        scrollContainer.CreateTextureContentToSprite(lureTextures);
    }

    private void UpdateColoring(Color baseC, Color textC, int selectedId)
    {
        baseColor = baseC;
        baseColorButton.color = baseColor;

        textureColor = textC;
        textureColorButton.color = textureColor;

        // Access the MeshRenderer's materials array
        Material[] materials = blockRenderer.materials;

        // Iterate through the materials and modify them
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetColor(BASE_COLOR, baseColor);
            materials[i].SetColor(TEX_COLOR, textureColor);
            materials[i].SetTexture("_OverlayTex", lureTextures[selectedId]);
        }

        // Assgin the materials
        blockRenderer.materials = materials;
        selectedTextureId = selectedId;
        scrollContainer.UpdateColors(baseColor, textureColor);
    }

    public void Activate()
    {
        Material[] mats = blockRenderer.materials;
        for (int i = 0; i < mats.Length; i++)
        {
            mats[i] = coloredBlockMaterial;
        }
        blockRenderer.materials = mats;
        UpdateColoring(Color.white, Color.black, selectedTextureId);
    }

    public void UpdateBaseColor()
    {
        colorWheel.ActivateColorPicker(baseColorButton, baseColor, BASE_COLOR);
    }

    public void UpdateTexColor()
    {
        colorWheel.ActivateColorPicker(textureColorButton, textureColor, TEX_COLOR);
    }

    public void AcceptColoring()
    {
        UpdateColoring(baseColorButton.color, textureColorButton.color, selectedTextureId);
    }

    public void DeclineColoring()
    {
        UpdateColoring(baseColor, textureColor, selectedTextureId);
    }

    public void SelectTexture(int id)
    {
        UpdateColoring(baseColor, textureColor, id);
    }
}
