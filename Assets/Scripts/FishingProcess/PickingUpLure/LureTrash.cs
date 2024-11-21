using UnityEngine;

public class LureTrash : MonoBehaviour
{
    [SerializeField] private Outline outline;

    private void Start()
    {
        outline.enabled = false;
    }

    private void OnMouseDown()
    {
        if (FishManager.Instance.CanFish && FishManager.Instance.IsHoldingLure)
        {
            FishManager.Instance.DeleteLure();
            outline.enabled = false;
        }
    }

    private void OnMouseEnter()
    {
        if (FishManager.Instance.CanFish && FishManager.Instance.IsHoldingLure)
        {
            outline.enabled = true;
        }
    }

    private void OnMouseExit()
    {
        outline.enabled = false;
    }
}
