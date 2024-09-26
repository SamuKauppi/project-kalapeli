using System.Buffers.Text;
using UnityEngine;

public class ScrollTextureContainer : MonoBehaviour
{
    [SerializeField] private BlockPainter blockPainter;
    [SerializeField] private ScrollTextureObject contentPrefab;
    [SerializeField] private RectTransform contentParent;

    private ScrollTextureObject[] contentImages = null;
    public void CreateContent(Texture2D[] textures)
    {
        if (contentImages != null)
            return;

        contentImages = new ScrollTextureObject[textures.Length];
        for (int i = 0; i < textures.Length; i++)
        {
            contentImages[i] = Instantiate(contentPrefab, contentParent);
            contentImages[i].displayImage.sprite = Sprite.Create(textures[i],
                                                                 new Rect(0, 0, -textures[i].width, textures[i].height),
                                                                 new Vector2(0.5f, 0.5f));

            contentImages[i].arrayIndex = i;
        }

        contentParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                                                contentImages.Length * (contentImages[0].displayImage.rectTransform.rect.height + 80f));

        transform.GetChild(0).gameObject.SetActive(false);
    }

    public void UpdateColors(Color baseC, Color texC)
    {
        for (int i = 0; i < contentImages.Length; i++)
        {
            contentImages[i].displayImage.color = texC;
            contentImages[i].backgroundImage.color = baseC;
        }
    }
}
