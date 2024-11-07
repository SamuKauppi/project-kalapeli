using UnityEngine;

public class PickUpLure : MonoBehaviour
{
    [SerializeField] private Outline outline;

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
            outline.enabled = true;
    }

    private void OnMouseExit()
    {
        outline.enabled = false;
    }
}
