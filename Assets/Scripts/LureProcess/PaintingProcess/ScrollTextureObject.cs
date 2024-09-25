using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Attached to the prefab we create during runtime
/// </summary>
public class ScrollTextureObject : MonoBehaviour
{
    public Button SelectionButton;
    public Image displayImage;
    public Image backgroundImage;
    public int arrayIndex;

    public void Start()
    {
        SelectionButton.onClick.AddListener(() => BlockPainter.Instance.SelectTexture(arrayIndex));
    }
}
