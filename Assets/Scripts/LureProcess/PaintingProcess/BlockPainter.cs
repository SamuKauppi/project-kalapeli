using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class that handles changing color of a shader
/// </summary>
public class BlockPainter : MonoBehaviour
{
    /// <summary>
    /// Singleton
    /// </summary>
    public static BlockPainter Instance { get; private set; }

    // Event for changing color
    public delegate void ChangeColor(Color baseC, Color texC, int textureID);
    public static event ChangeColor OnColorChange;

    // Object references
    [SerializeField] private ScrollTextureContainer scrollContainer;
    [SerializeField] private ColorWheel colorWheel;
    [SerializeField] private Image baseColorButton;
    [SerializeField] private Image textureColorButton;
    private MeshRenderer blockRenderer;

    // Matrial properties
    [SerializeField] private Texture2D[] lureTextures;
    [SerializeField] private Material coloredBlockMaterial;
    [SerializeField] private int defaultPaintID;

    // Coloring variables
    private Color baseColor = Color.white;
    private Color textureColor = Color.black;
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
        blockRenderer = BlockRotation.Instance.GetComponent<MeshRenderer>();
        selectedTextureId = defaultPaintID;
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
        OnColorChange?.Invoke(baseColor, textureColor, selectedTextureId);
    }

    public void Activate()
    {
        Material[] mats = blockRenderer.materials;
        for (int i = 0; i < mats.Length; i++)
        {
            mats[i] = coloredBlockMaterial;
        }
        blockRenderer.materials = mats;
        UpdateColoring(baseColor, textureColor, selectedTextureId);
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
        SoundManager.Instance.PlaySound(SoundClipTrigger.OnPaint);
    }

    public void DeclineColoring()
    {
        UpdateColoring(baseColor, textureColor, selectedTextureId);
        SoundManager.Instance.PlaySound(SoundClipTrigger.OnUiError);
    }

    public void SelectTexture(int id)
    {
        UpdateColoring(baseColor, textureColor, id);
    }

    public Sprite GetTextureSprites(int id)
    {
        return Sprite.Create(lureTextures[id],
                                       new Rect(0, 0, -lureTextures[id].width, lureTextures[id].height),
                                       new Vector2(0.5f, 0.5f));
    }
}
