using UnityEngine.UI;

public class ScrollTextureObject : ScrollObject
{
    public Image backgroundImage;
    protected override void Start()
    {
        SelectionButton.onClick.AddListener(() => BlockPainter.Instance.SelectTexture(arrayIndex));
        SelectionButton.onClick.AddListener(() => SoundManager.Instance.PlaySound(SoundClipTrigger.OnPaint));
    }
}
