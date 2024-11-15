using UnityEngine;

public class PickUpLure : MonoBehaviour
{
    [SerializeField] private Outline outline;
    [SerializeField] private float onWidth;
    [SerializeField] private float offWidth;
    [SerializeField] private GameObject[] luresInBox;
    private void Start()
    {
        if (outline == null)
            outline = GetComponent<Outline>();

        outline.enabled = false;
    }

    private void OnMouseDown()
    {
        if (FishManager.Instance.CanFish)
            FishManager.Instance.PickUpLure();
    }
    private void OnMouseEnter()
    {
        if (FishManager.Instance.CanFish)
            outline.OutlineWidth = onWidth;
    }

    private void OnMouseExit()
    {
        outline.OutlineWidth = offWidth;
    }

    public void OpenLureBox(int lureCount)
    {
        outline.enabled = lureCount > 0;
        for (int i = 0; i < luresInBox.Length; i++)
        {
            luresInBox[i].SetActive(lureCount > 0);
        }
    }
}
