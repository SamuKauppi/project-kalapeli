using UnityEngine;

public class ClickObject : MonoBehaviour
{
    [SerializeField] private SoundClipTrigger trigger;

    private void OnMouseDown()
    {
        SoundManager.Instance.PlaySound(trigger);
    }
}
