using UnityEngine;
using UnityEngine.UI;

public class DisplayTemplate : MonoBehaviour
{
    public static DisplayTemplate Instance { get; private set; }

    [SerializeField] private Image templateImage;
    [SerializeField] private TemplateSprites[] templateSprites;
    [SerializeField] private ScrollTemplateContainer scrollContainer;

    int templateIndex;
    int spriteIndex;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        BlockRotation.OnRotation += ChangeTemplate;
    }
    private void OnDisable()
    {
        BlockRotation.OnRotation -= ChangeTemplate;
    }

    private void Start()
    {
        ChangeTemplate(0, 0);
        Sprite[] sprites = new Sprite[templateSprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i] = templateSprites[i].sprites[0];
        }
        scrollContainer.CreateTemplateContent(sprites);
    }

    private void ChangeTemplate(int sideRot, int upRot)
    {
        int id = upRot > 0 ? sideRot + (4 * upRot) : sideRot;
        templateImage.sprite = templateSprites[templateIndex].sprites[id];
        spriteIndex = id;
    }

    public void ChangeTemplate(int id)
    {
        templateIndex = id;
        templateImage.sprite = templateSprites[templateIndex].sprites[spriteIndex];
    }
}

[System.Serializable]
public class TemplateSprites
{
    public Sprite[] sprites;
}
