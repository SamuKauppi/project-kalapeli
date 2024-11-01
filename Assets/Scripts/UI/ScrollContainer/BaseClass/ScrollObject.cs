using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Attached to the prefab we create during runtime
/// USed to display options for multiple things
/// </summary>
public class ScrollObject : MonoBehaviour
{
    public Image displayImage;
    public Button SelectionButton;
    public int arrayIndex;

    protected virtual void Start() { }
}
