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
        FishManager.Instance.PickUpLure();
    }
    private void OnMouseEnter()
    {
        outline.enabled = true;
    }

    private void OnMouseExit()
    {
        outline.enabled = false;
    }
}
