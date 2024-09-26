using UnityEngine;

public class ScrollContainer : MonoBehaviour
{
    [SerializeField] protected ScrollObject contentPrefab;
    [SerializeField] protected RectTransform contentParent;
    [SerializeField] protected float padding = 40f;

    protected ScrollObject[] contentImages = null;
    protected void CreateContent(Sprite[] sprites)
    {
        if (contentImages != null)
            return;

        contentImages = new ScrollObject[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            contentImages[i] = Instantiate(contentPrefab, contentParent);
            contentImages[i].displayImage.sprite = sprites[i];

            contentImages[i].arrayIndex = i;
        }

        contentParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                                                contentImages.Length * (contentImages[0].displayImage.rectTransform.rect.height + (padding * 2f)));

        transform.GetChild(0).gameObject.SetActive(false);
    }
}
