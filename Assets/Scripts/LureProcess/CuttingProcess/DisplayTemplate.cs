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
        BlockRotation.OnRotationStart += ChangeTemplate;
    }
    private void OnDisable()
    {
        BlockRotation.OnRotationStart -= ChangeTemplate;
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
        ScaleTemplateImage();
    }

    private void ChangeTemplate(int sideRot, int upRot)
    {
        int id;
        if (upRot == 0)
        {
            // If looking from side, use side templates
            id = sideRot;
        }
        else if (upRot < 0)
        {
            // If looking from above, use templates from above
            id = sideRot + 4;
        }
        else
        {
            // If looking from below, use templated from above but swap 1 and 3
            sideRot = sideRot == 3 ? 1 : sideRot == 1 ? 3 : sideRot;
            id = sideRot + 4;
        }

        templateImage.sprite = templateSprites[templateIndex].sprites[id];
        spriteIndex = id;
    }

    private void ScaleTemplateImage()
    {
        float objWidth = BlockRotation.Instance.GetComponent<Renderer>().bounds.size.x;
        Vector3 screenPosition = GameManager.Instance.LureCamera.WorldToScreenPoint(BlockRotation.Instance.transform.position);
        Vector3 rightEdge = GameManager.Instance.LureCamera.WorldToScreenPoint(BlockRotation.Instance.transform.position
            + new Vector3(objWidth, 0, 0));
        float screenWidth = Mathf.Abs(rightEdge.x - screenPosition.x);

        templateImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, screenWidth);
        templateImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, screenWidth);
    }

    public void ChangeTemplate(int id)
    {
        templateIndex = id;
        templateImage.sprite = templateSprites[templateIndex].sprites[spriteIndex];
    }

    public void ShowTemplate(bool value)
    {
        templateImage.gameObject.SetActive(value);
    }
}

[System.Serializable]
public class TemplateSprites
{
    public Sprite[] sprites;
}
